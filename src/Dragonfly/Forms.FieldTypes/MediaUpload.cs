namespace Dragonfly.Forms.FieldTypes
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Umbraco.Core;
    using Umbraco.Core.IO;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Models;
    using Umbraco.Forms.Core;
    using Umbraco.Web;

    //Based partially on code from https://our.umbraco.org/forum/umbraco-pro/contour/40579-Upload-to-Media-custom-FieldType-example

    public class MediaUpload : Umbraco.Forms.Core.Providers.FieldTypes.FileUpload
    {
        const int ImageMediaTypeId = 1032;

        [Umbraco.Forms.Core.Attributes.Setting("Allowed file extensions",
             description = "Allowed file extensions (CSV)",
             alias = "AllowedExtensions")]
        public string AllowedExtensions { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Media Parent ID",
             description = "ID of the parent where the media will be saved",
             alias = "MediaParentId")]
        public string MediaParentId { get; set; }

        [Umbraco.Forms.Core.Attributes.Setting("Media Parent Path",
             description = "Courier-friendly 'Path' of Media Node names to the parent where the media will be saved - use format 'Media Name 1>Media Name 2>Name 3'",
             alias = "MediaParentPath")]
        public string MediaParentPath { get; set; }

        public MediaUpload()
        {
            // Mandatory
            this.Id = new Guid("84f24c36-103d-4161-b2ec-058e57e315f2");
            this.Name = "Media Upload";
            this.Description = "Upload file to Umbraco Media Section";
            this.Icon = "icon-cloud-upload";
            this.DataType = FieldDataType.Integer;
            //this.FieldTypeViewName = "FieldType.xxx.cshtml";

            // Optional         
            this.Category = "Custom Types";
            this.HideField = false;
            this.HideLabel = false;
            this.SortOrder = 10;
            this.SupportsPrevalues = false;
            //this.SupportsRegex = true;
        }


        public override IEnumerable<object> ProcessSubmittedValue(Umbraco.Forms.Core.Field AssociatedField, System.Collections.Generic.IEnumerable<object> Values, System.Web.HttpContextBase HttpContext)
        {
            List<Object> vals = new List<object>();

            //files
            var ms = ApplicationContext.Current.Services.MediaService;
            var cts = ApplicationContext.Current.Services.ContentTypeService;
            bool filesaved = false;
            string _text;
            string mediaPath = "";
            Media m = null;
            var files = HttpContext.Request.Files;
            if (files.Count > 0 && files.AllKeys.Contains(AssociatedField.Id.ToString()))
            {
                HttpPostedFileBase file = null;
                file = files[AssociatedField.Id.ToString()];
                if (file.ContentLength > 0)
                {
                    if (file.FileName != "")
                    {
                        // Find filename
                        _text = file.FileName;
                        string filename;
                        string _fullFilePath;

                        filename = _text.Substring(_text.LastIndexOf("\\") + 1, _text.Length - _text.LastIndexOf("\\") - 1).ToLower();

                        // create the Media Node

                        var mediaType = cts.GetMediaType(ImageMediaTypeId);
                        var mediaParentId = this.GetParentId(AssociatedField);
                        m = new Media(
                            filename, mediaParentId, mediaType);

                        ms.Save(m);

                        // Create a new folder in the /media folder with the name /media/propertyid
                        string mediaRootPath = "~/media/"; // get path from App_GlobalResources
                        string storagePath = mediaRootPath + m.Id.ToString();

                        if (!System.IO.Directory.Exists(IOHelper.MapPath(storagePath)))
                            System.IO.Directory.CreateDirectory(IOHelper.MapPath(storagePath));

                        _fullFilePath = IOHelper.MapPath(storagePath) + "\\" + filename;
                        file.SaveAs(_fullFilePath);

                        // Save extension
                        string orgExt = ((string) _text.Substring(_text.LastIndexOf(".") + 1, _text.Length - _text.LastIndexOf(".") - 1));
                        orgExt = orgExt.ToLower();
                        string ext = orgExt.ToLower();
                        try
                        {
                            m.SetValue("umbracoExtension", ext);
                        }
                        catch {}

                        // Save file size
                        try
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(_fullFilePath);
                            m.SetValue("umbracoBytes", fi.Length.ToString());
                        }
                        catch {}

                        // Check if image and then get sizes, make thumb and update database
                        if (",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + ext + ",") > 0)
                        {
                            int fileWidth;
                            int fileHeight;

                            FileStream fs = new FileStream(_fullFilePath,
                                FileMode.Open, FileAccess.Read, FileShare.Read);

                            System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
                            fileWidth = image.Width;
                            fileHeight = image.Height;
                            fs.Close();
                            try
                            {
                                m.SetValue("umbracoWidth", fileWidth.ToString());
                                m.SetValue("umbracoHeight", fileHeight.ToString());
                            }
                            catch {}

                            // Generate thumbnails
                            string fileNameThumb = _fullFilePath.Replace("." + orgExt, "_thumb");
                            GenerateThumbnail(image, 100, fileWidth, fileHeight, _fullFilePath, ext, fileNameThumb + ".jpg");

                            image.Dispose();
                        }
                        mediaPath = "/media/" + m.Id.ToString() + "/" + filename;

                        m.SetValue("umbracoFile", mediaPath);

                        ms.Save(m);

                        vals.Add(m.Id);

                        filesaved = true;
                    }
                }

            }

            if (!filesaved)
            {
                vals.Add("No file saved");
            }

            return vals;
        }

        private int GetParentId(Umbraco.Forms.Core.Field AssociatedField)
        {
            var parentId = -1; //media root :: node = -1

            if (!this.MediaParentId.IsNullOrWhiteSpace())
            {
                try
                {
                    parentId = Convert.ToInt32(this.MediaParentId);
                }
                catch (Exception ex1)
                {
                    var msg = string.Format("Provided property value for 'MediaParentId' on field '{0}' on Form is not a valid Integer.", AssociatedField.Alias);
                    LogHelper.Error<MediaUpload>(msg, ex1);
                }
            }
            else if (!this.MediaParentPath.IsNullOrWhiteSpace())
            {
                try
                {
                    var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                    var pathSplit = this.MediaParentPath.Split('>');
                    var mediaParent = umbracoHelper.TypedMediaAtRoot().Where(m => m.Name == pathSplit[0]).FirstOrDefault();

                    for (int i = 1; i < pathSplit.Length - 1; i++)
                    {
                        var nodeName = pathSplit[i];
                        mediaParent = mediaParent.Children.Where(m => m.Name == nodeName).FirstOrDefault();
                    }

                    parentId = mediaParent.Id;
                }
                catch (Exception ex2)
                {
                    var msg = string.Format("Provided property value for 'MediaParentPath' on field '{0}' on Form caused an error.", AssociatedField.Alias);
                    LogHelper.Error<MediaUpload>(msg, ex2);
                }
            }

            return parentId;
        }

        public override IEnumerable<object> ConvertToRecord(Field Field, IEnumerable<object> PostedValues, HttpContextBase Context)
        {
            List<Object> vals = new List<object>();

            if (PostedValues.Any())
            {
                var firstValue = "";
                try
                {
                    firstValue = PostedValues.First().ToString();
                    vals.Add(Convert.ToInt32(firstValue));
                }
                catch (Exception)
                {
                    vals.Add(0);
                }
                

                //Get media file path
                //var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                //var mediaId = Convert.ToInt32(postedValues.First().ToString());
                //var mediaItem = umbracoHelper.TypedMedia(mediaId);
                //var mediaFullPath = mediaItem.Url;
                //vals.Add(mediaFullPath);
            }

            return vals;
        }


        protected void GenerateThumbnail(System.Drawing.Image image, int maxWidthHeight, int fileWidth, int fileHeight, string fullFilePath, string ext, string thumbnailFileName)
        {
            // Generate thumbnail
            float fx = (float)fileWidth / (float)maxWidthHeight;
            float fy = (float)fileHeight / (float)maxWidthHeight;
            // must fit in thumbnail size
            float f = Math.Max(fx, fy); //if (f < 1) f = 1;
            int widthTh = (int)Math.Round((float)fileWidth / f); int heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            Bitmap bp = new Bitmap(widthTh, heightTh);
            Graphics g = Graphics.FromImage(bp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Copy the old image to the new and resized
            Rectangle rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            // Copy metadata
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType.Equals("image/jpeg"))
                    codec = codecs[i];
            }

            // Set compresion ratio to 90%
            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

            // Save the new image
            bp.Save(thumbnailFileName, codec, ep);
            bp.Dispose();
            g.Dispose();

        }


    }
}
