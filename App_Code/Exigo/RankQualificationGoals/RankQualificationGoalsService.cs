using Exigo.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Exigo.RankQualificationGoals
{
    public class RankQualificationGoalsService
    {
        public RankQualificationGoalsService()
        {

        }        

        public GetRankGoalificationGoalsResponse GetRankQualificationGoals(int customerID, int rankID)
        {
            // Get the rank qualifications from the API
            var apiResponse = new GetRankQualificationsResponse();
            var qualificationResponses = new QualificationResponse[0];
            var result = new GetRankGoalificationGoalsResponse();

            try
            {
                apiResponse = ExigoApiContext.CreateWebServiceContext().GetRankQualifications(new GetRankQualificationsRequest
                {
                    CustomerID = customerID,
                    PeriodType = (int)PeriodTypes.Default,
                    RankID = rankID
                });
                qualificationResponses = apiResponse.PayeeQualificationLegs[0];
            }
            catch
            {
                qualificationResponses = new QualificationResponse[0];
            }


            result.TotalPercentComplete = qualificationResponses.GetPercentComplete();
            result.RankID = apiResponse.RankID;
            result.RankDescription = apiResponse.RankDescription;
            result.PreviousRankID = apiResponse.BackRankID;
            result.PreviousRankDescription = apiResponse.BackRankDescription;
            result.NextRankID = apiResponse.NextRankID;
            result.NextRankDescription = apiResponse.NextRankDescription;


            // Assemble the rank qualifications
            var expression          = string.Empty;
            var qualification       = new RankQualification();
            var results             = new List<RankQualification>();


            // Customer Type
            expression                                          = @"^MUST BE A VALID DISTRIBUTOR TYPE";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Customer type";
            qualification.IsBoolean                             = true;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = "You must be a distributor in order to qualify for the next rank.";
            results.Add(qualification);


            // Customer Status
            expression                                          = @"CUSTOMER STATUS IS ACTIVE";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Active status";
            qualification.IsBoolean                             = true;                
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = "You must have an Active status in order to qualify for the next rank.";
            results.Add(qualification);


            // Active
            expression                                          = @"^ACTIVE$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Active";
            qualification.IsBoolean                             = true;                
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = "You must be Active in order to qualify for the next rank.";
            results.Add(qualification);


            // Qualified
            expression                                          = @"^MUST BE QUALIFIED$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Qualified";
            qualification.IsBoolean                             = true;                
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = "You must be qualified for commissions in order to advance to the next rank.";
            results.Add(qualification);


            // Enroller Tree
            expression                                          = @"^DISTRIBUTOR MUST BE IN ENROLLER TREE$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Enroller Tree";
            qualification.IsBoolean                             = true;  
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = "You must have a current position in the enroller tree in order to advance to the next rank.";
            results.Add(qualification);


            // Lesser Leg Volume
            expression                                          = @"^\d+ LESSER LEG VOLUME$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Lesser Leg Volume";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("You need at least <strong>{0:N0}</strong> more volume in your lesser leg.", qualification.AmountNeededToQualify);
            results.Add(qualification);
            

            // C500 Legs
            expression                                          = @"^\d+ C500 LEGS ENROLLER TREE$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "C500 Legs in Enroller Tree";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("You need <strong>{0:N0} more C500 distributor(s) in your enroller tree</strong> to advance to the next rank.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // PV
            expression                                          = @"^\d+ PV$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "PV";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("You need <strong>{0:0} more PV</strong> to advance.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // PV 1 Period Ago
            expression                                          = @"^\d+ PV 1 PERIOD";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "PV Last Period";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your last period didn't have enough PV to advance you.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // PV 2 Period Ago
            expression                                          = @"^\d+ PV 2 PERIOD";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "PV 2 Periods Ago";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your PV from two periods ago didn't have enough PV to advance you.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // PV 3 Period Ago
            expression                                          = @"^\d+ PV 3 PERIOD";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "PV 3 Periods Ago";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your PV from three periods ago didn't have enough PV to advance you.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // Capped Enrollment Group PV at 50% Per Leg
            expression                                          = @"^\d+ CAPPED ENROLLMENT GROUP PV AT 50% PER LEG$";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Capped Enrollment GPV at 50% per leg";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your current capped enrollment GPV at 50% per leg is insufficient. You need <strong>{0:N0} more</strong> to advance.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // Capped Enrollment Group PV at 50% Per Leg 1 Period Ago
            expression                                          = @"^\d+ CAPPED ENROLLMENT GROUP PV AT 50% PER LEG 1 PERIOD";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Capped Enrollment GPV at 50% per leg last period";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your current capped enrollment GPV at 50% per leg last period was insufficient. You needed <strong>{0:N0} more</strong> to advance.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // Capped Enrollment Group PV at 50% Per Leg 2 Periods Ago
            expression                                          = @"^\d+ CAPPED ENROLLMENT GROUP PV AT 50% PER LEG 2 PERIOD";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Capped Enrollment GPV at 50% per leg two periods ago";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your current capped enrollment GPV at 50% per leg two periods ago was insufficient. You needed <strong>{0:N0} more</strong> to advance.", qualification.AmountNeededToQualify);
            results.Add(qualification);


            // Capped Enrollment Group PV at 50% Per Leg 3 Periods Ago
            expression                                          = @"^\d+ CAPPED ENROLLMENT GROUP PV AT 50% PER LEG 3 PERIOD";
            qualification                                       = new RankQualification(qualificationResponses, expression);
            qualification.Label                                 = "Capped Enrollment GPV at 50% per leg three periods ago";
            qualification.IsBoolean                             = false;
            qualification.QualifiedDescription                  = "";
            qualification.NotQualifiedDescription               = string.Format("Your current capped enrollment GPV at 50% per leg three periods ago was insufficient. You needed <strong>{0:N0} more</strong> to advance.", qualification.AmountNeededToQualify);
            results.Add(qualification);

           


            // Clean up nulls
            results.RemoveAll(c => string.IsNullOrEmpty(c.RequiredValue));
            result.RankQualifications = results;


            return result;
        }
    }   

    #region Models and Enums
    public enum RankQualificationGroup
    {
        None,
        PV,
        CappedEnrollmentGroupPV
    }
    public class GetRankGoalificationGoalsResponse
    {
        public List<RankQualification> RankQualifications { get; set; }
        public decimal TotalPercentComplete { get; set; }
        public int RankID { get; set; }
        public string RankDescription { get; set; }
        public int? PreviousRankID { get; set; }
        public string PreviousRankDescription { get; set; }
        public int? NextRankID { get; set; }
        public string NextRankDescription { get; set; }
    }

    public class RankQualification
    {
        public RankQualification()
        {

        }
        public RankQualification(QualificationResponse[] qualifications, string expression)
        {
            var response = GetQualificationByDescription(qualifications, expression);
            Expression = expression;

            if(response != null)
            {
                QualificationResponse = response;
                RequiredValue = response.Required;
                ActualValue = response.Actual;
                IsOverridden = (response.QualifiesOverride != null) ? Convert.ToBoolean(response.QualifiesOverride) : false;
                QualifiedValue = response.Qualifies;
            }
        }

        public QualificationResponse QualificationResponse { get; set; }
        public string Expression { get; set; }
        public string RequiredValue { get; set; }
        public string ActualValue { get; set; }
        public bool QualifiedValue { get; set; }
        public bool IsBoolean { get; set; }
        public bool IsOverridden { get; set; }
        public RankQualificationGroup Group { get; set; }
        public int GroupPriority { get; set; }

        public string QualifiedDescription { get; set; }
        public string NotQualifiedDescription { get; set; }

        public string Label { get; set; }
        public bool IsQualified
        {
            get
            {
                return QualifiedValue || IsOverridden;
            }
        }
        public string CurrentDescription 
        {
            get
            {
                if (this.IsQualified) return QualifiedDescription;
                else return NotQualifiedDescription;
            }
        }
        public decimal RequiredValueAsDecimal
        {
            get
            {
                if (!IsBoolean) return GlobalUtilities.TryParse<decimal>(this.RequiredValue, 0M);
                else return -1M;
            }
        }
        public decimal ActualValueAsDecimal
        {
            get
            {
                if (!IsBoolean) return GlobalUtilities.TryParse<decimal>(this.ActualValue, 0M);
                else return -1M;
            }
        }
        public decimal RequiredToActualAsRatio
        {
            get
            {
                if (this.RequiredValueAsDecimal > 0) 
                {                
                    return (this.ActualValueAsDecimal / this.RequiredValueAsDecimal);
                }
                else return this.RequiredValueAsDecimal;
            }
        }
        public decimal RequiredToActualAsPercent
        {
            get
            {
                if(this.RequiredToActualAsRatio > 1M) return 100M;
                else return this.RequiredToActualAsRatio * 100;
            }
        }
        public decimal AmountNeededToQualify
        {
            get
            {
                return this.RequiredValueAsDecimal - this.ActualValueAsDecimal;
            }
        }
        public decimal ExcessAmountOverRequired
        {
            get
            {
                return this.ActualValueAsDecimal - this.RequiredValueAsDecimal;
            }
        }

        public string GetProgressBarColorClass()
        {
            var percentComplete = RequiredToActualAsPercent;
            if (percentComplete <= 60) return "progress-danger";
            else if (percentComplete <= 80) return "progress-warning";
            else return "progress-success";
        }

        #region Helper Methods
        private QualificationResponse GetQualificationByDescription(QualificationResponse[] qualifications, string expression)
        {
            Regex regex = new Regex(expression);
            QualificationResponse qualification = null;

            qualification = (from q in qualifications
                                 let description = q.QualificationDescription.ToUpper()
                                 let matches = regex.Matches(description)
                                 where matches.Count > 0
                                 select q).FirstOrDefault();

            return qualification;
        }
        #endregion
    }
    #endregion
}