namespace Dragonfly.Forms.Helpers
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Dragonfly.Forms.Models;

    public static class NetPromoterHelper
    {
        public static NetPromoterRating RatingFromJson(string RatingJson)
        {
            NetPromoterRating npRating = JsonConvert.DeserializeObject<NetPromoterRating>(RatingJson);

            return npRating;
        }

        public static IEnumerable<NetPromoterRating> RatingsfromFormRecords(string FormGuid, string NpsFieldAlias)
        {
            var formData = new FormWithRecords(FormGuid);
            return RatingsfromFormRecords(formData, NpsFieldAlias);
        }

        public static IEnumerable<NetPromoterRating> RatingsfromFormRecords(FormWithRecords FormData, string NpsFieldAlias)
        {
            var npsDataRaw = FormData.AllFieldData(NpsFieldAlias);
            var npsDataSet = new List<NetPromoterRating>();

            foreach (var datapoint in npsDataRaw)
            {
                npsDataSet.Add(RatingFromJson(datapoint.Value.ValuesAsString()));
            }

            return npsDataSet;
        }

        public static NetPromoterScore GetNetPromoterScore(IEnumerable<NetPromoterRating> Ratings)
        {
            return new NetPromoterScore(Ratings);
        }

        public static NetPromoterScore GetNetPromoterScore(FormWithRecords FormData, string NpsFieldAlias)
        {
            var ratings = RatingsfromFormRecords(FormData, NpsFieldAlias);
            return new NetPromoterScore(ratings);
        }

        public static NetPromoterScore GetNetPromoterScore(string FormGuid, string NpsFieldAlias)
        {
            var ratings = RatingsfromFormRecords(FormGuid, NpsFieldAlias);
            return new NetPromoterScore(ratings);
        }
    }
}
