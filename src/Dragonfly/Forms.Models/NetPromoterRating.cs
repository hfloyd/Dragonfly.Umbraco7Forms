namespace Dragonfly.Forms.Models
{
    using Newtonsoft.Json;

    public class NetPromoterRating
    {
        public bool IsValid { get; set; }
        public int Value { get; set; }
        public string Category { get; set; }

        public NetPromoterRating()
        {
            
        }

        public NetPromoterRating(bool Invalid = true)
        {
            if (Invalid)
            {
                this.IsValid = false;
                this.Value = 0;
                this.Category = "Invalid Entry";
            }
        }

        public NetPromoterRating(int Value)
        {
            this.Value = Value;

            //The people who answer on the “Not Likely” end of the spectrum (0 to 6) are considered “Detractors,” 
            //while people whose answers fall in the 7 to 8 range are called “Passives.” 
            //Those who answer in the “Very Likely” range (9 or 10) are referred to as “Promoters.”
            if (Value < 7)
            {
                this.Category = "Detractor";
                this.IsValid = true;
            }
            else if (Value <= 8)
            {
                this.Category = "Passive";
                this.IsValid = true;
            }
            else if (Value > 8)
            {
                this.Category = "Promoter";
                this.IsValid = true;
            }
            else
            {
                this.IsValid = false;
                this.Category = "Invalid Entry";
            }
        }

        public string ToJson()
        {
            string json = JsonConvert.SerializeObject(this);
            return json;
        }

        public override string ToString()
        {
            return ToJson();
              //return string.Format("Value: {0}, Category: {1}, IsValid: {2}", Value, Category, IsValid);
        }
    }
}
