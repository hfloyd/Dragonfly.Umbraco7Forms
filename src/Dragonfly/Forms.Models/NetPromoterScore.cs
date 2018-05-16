namespace Dragonfly.Forms.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public class NetPromoterScore
    {
        public IEnumerable<NetPromoterRating> AllRatings { get; set; }

        public NetPromoterScore(IEnumerable<NetPromoterRating> Ratings)
        {
            this.AllRatings = Ratings;
        }


        public IEnumerable<NetPromoterRating> AllValidRatings()
        {
            return this.AllRatings.Where(n => n.IsValid);
        }

        public decimal GetScore()
        {
            //To calculate your official NPS® score, simply take the total percentage of Promoters and subtract the percentage of Detractors. 
            //You can then leave this metric as a percentage such as 43%, or change it to a whole number such as 43.
            if (this.AllValidRatings().Any())
            {
                decimal total = this.AllValidRatings().Count();

                decimal promotersCount = this.PromotersCount();
                decimal promotersPercentage = promotersCount / total;

                decimal detractorsCount = this.DetractorsCount();
                decimal detractorsPercentage = detractorsCount / total;

                decimal score = promotersPercentage - detractorsPercentage;

                return (score * 100);
            }
            else
            {
                return 0;
            }
        }


        public NpsScoreCategory GetScoreCategory()
        {
            var npsScore = this.GetScore();

            var npsCats = new NpsScoreCategories();

            foreach (var catOption in npsCats.Categories.OrderBy(n=> n.Order))
            {
                if (npsScore <= catOption.TopValue)
                {
                    return catOption;
                }
            }


            //if (npsScore <= 0)
            //{
            //    returnDict.Add(0, "Poor");
            //}
            //else if (npsScore > 0 & npsScore < 50)
            //{
            //    returnDict.Add(1, "Good");
            //}
            //if (npsScore > 49 & npsScore < 70)
            //{
            //    returnDict.Add(2, "Excellent");
            //}
            //if (npsScore >= 70)
            //{
            //    returnDict.Add(3, "World Class");
            //}

            //if we get here, something is wrong.
            return new NpsScoreCategory()
            {
                Order = -1,
                CategoryName = "Invalid",
                TopValue = 0,
                PercentageOfRange = 0,
                ColorName = "Black",
                ColorHex = "#000000"
            };

        }

        public int PromotersCount()
        {
            return AllRatings.Count(n => n.Category == "Promoter");
        }

        public int DetractorsCount()
        {
            return AllRatings.Count(n => n.Category == "Detractor");
        }

        public int PassivesCount()
        {
            return AllRatings.Count(n => n.Category == "Passive");
        }

        public int InvalidCount()
        {
            return AllRatings.Count(n => !n.IsValid);
        }

    }

    public class NpsScoreCategories
    {
        public IEnumerable<NpsScoreCategory> Categories { get; set; }
        public int MinValue = -100;
        public int MaxValue = 100;

        public NpsScoreCategories()
        {
            //Given the NPS range of -100 to +100, a “positive” score or NPS above 0 is considered “good”, 
            //+50 is “Excellent,” and above 70 is considered “world class.” 

            var cats = new List<NpsScoreCategory>();
            cats.Add(new NpsScoreCategory() { Order = 0, CategoryName = "Poor", TopValue = 0, PercentageOfRange = .5, ColorName = "Red", ColorHex = "#DF5353" });
            cats.Add(new NpsScoreCategory() { Order = 1, CategoryName = "Good", TopValue = 49, PercentageOfRange = .75, ColorName = "Yellow", ColorHex = "#DDDF0D" }); //"Yellow", ColorHex = "#DDDF0D"
            cats.Add(new NpsScoreCategory() { Order = 2, CategoryName = "Excellent", TopValue = 69, PercentageOfRange = .85, ColorName = "Green", ColorHex = "#55BF3B" });
            cats.Add(new NpsScoreCategory() { Order = 3, CategoryName = "World Class", TopValue = 100, PercentageOfRange = 1, ColorName = "Blue", ColorHex = "#639CD3" }); // "Purple", ColorHex = "#676CD0"

            this.Categories = cats;
        }
    }
    
    public class NpsScoreCategory
    {
        public string CategoryName { get; internal set; }
        public int Order { get; internal set; }
        public int TopValue { get; internal set; }
        public double PercentageOfRange { get; internal set; }
        public string ColorName { get; internal set; }
        public string ColorHex { get; internal set; }
        

        internal NpsScoreCategory()
        {
            
        }
    }
}
