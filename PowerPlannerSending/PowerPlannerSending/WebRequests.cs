using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ToolsPortable;
using ToolsPortable.Sql;

namespace PowerPlannerSending
{
    public static class GetItemErrors
    {
        public static readonly string NOT_FOUND = "Not found";
    }

    [DataContract]
    public class ItemRequest : WebsiteRequest
    {
        [DataMember]
        public Guid Identifier;
    }

    [DataContract]
    public class GetHomeworkRequest : ItemRequest
    {
    }

    [DataContract]
    public class GetHomeworkResponse : PlainResponse
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public string Name;

        [DataMember]
        public string Details;

        [DataMember]
        public DateTime Date;

        [DataMember]
        public double PercentComplete;

        [DataMember]
        public string[] ImageNames;

        [DataMember]
        public Guid ClassIdentifier;

        [DataMember]
        public string ClassName;

        [DataMember]
        public string ClassColor;
    }


    [DataContract]
    public class GetExamResponse : PlainResponse
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public string Name;

        [DataMember]
        public string Details;

        [DataMember]
        public DateTime Date;

        [DataMember]
        public DateTime Reminder;

        [DataMember]
        public string[] ImageNames;

        [DataMember]
        public Guid ClassIdentifier;

        [DataMember]
        public string ClassName;

        [DataMember]
        public string ClassColor;
    }

    [DataContract]
    public class GetClassRequest : WebsiteRequest
    {
        [DataMember]
        public Guid ClassIdentifier;

        [DataMember]
        public DateTime Today;
    }

    [DataContract]
    public class GetClassResponse : PlainResponse
    {
        [DataMember]
        public string Name;

        [DataMember]
        public string Color;

        [DataMember]
        public string Details;

        [DataMember]
        public IEnumerable<BasicScheduleInfo> Schedules;

        [DataMember]
        public IEnumerable<ListItemExamInfo> Exams;

        [DataMember]
        public IEnumerable<ListItemHomeworkInfo> Homework;

        // TODO: Grades
    }

    [DataContract]
    public class GetClassesRequest : WebsiteRequest
    {
        [DataMember]
        public Guid SemesterIdentifier;
    }

    [DataContract]
    public class GetClassesResponse : PlainResponse
    {
        [DataContract]
        public class Class
        {
            [DataMember]
            public Guid Identifier;

            [DataMember]
            public string Name;

            [DataMember]
            public string Color;
        }

        [DataMember]
        public IEnumerable<Class> Classes;
    }

    [DataContract]
    public class GetItemsForRangeRequest : WebsiteRequest
    {
        [DataMember]
        public Guid SemesterIdentifier;

        /// <summary>
        /// Inclusive
        /// </summary>
        [DataMember]
        public DateTime StartDate;

        /// <summary>
        /// Inclusive
        /// </summary>
        [DataMember]
        public DateTime EndDate;
    }

    [DataContract]
    public class GetItemsForRangeResponse : PlainResponse
    {
        [DataMember]
        public IEnumerable<ListItemExamInfoWithType> Items;
    }

    [DataContract]
    public class GetAgendaRequest : WebsiteRequest
    {
        [DataMember]
        public Guid SemesterIdentifier;

        [DataMember]
        public DateTime CurrentTime;
    }

    [DataContract]
    public class BasicClassInfoWithSchedules : BasicClassInfo
    {
        [DataMember]
        public IEnumerable<BasicScheduleInfo> Schedules { get; set; }
    }

    [DataContract]
    public class ListItemExamInfo
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public DateTime DateCreated;

        [DataMember]
        public string Name;

        [DataMember]
        public string ShortDetails;

        [DataMember]
        public DateTime Date;

        [DataMember]
        public Guid ClassIdentifier;
    }

    [DataContract]
    public class ListItemExamInfoWithType : ListItemExamInfo
    {
        [DataMember]
        public virtual ItemType ItemType
        {
            get { return ItemType.Exam; }
        }
    }

    [DataContract]
    public class ListItemMegaItem
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public DateTime DateCreated;

        [DataMember]
        public string Name;

        [DataMember]
        public string ShortDetails;

        [DataMember]
        public DateTime Date;

        [DataMember]
        public Guid UpperIdentifier;

        [DataMember]
        public MegaItemType MegaItemType;

        [DataMember]
        public double PercentComplete;
    }

    [DataContract]
    public class ListItemHomeworkInfo : ListItemExamInfo
    {
        [DataMember]
        public double PercentComplete;
    }

    [DataContract]
    public class ListItemHomeworkInfoWithType : ListItemExamInfoWithType
    {
        public override ItemType ItemType
        {
            get
            {
                return ItemType.Homework;
            }
        }

        [DataMember]
        public double PercentComplete;
    }

    [DataContract]
    public class BasicClassInfo
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public string Name;

        [DataMember]
        public string Color;
    }

    [DataContract]
    public class BasicScheduleInfo
    {
        [DataMember]
        public Guid Identifier;

        /// <summary>
        /// Not sent down
        /// </summary>
        public Guid ClassIdentifier;

        [DataMember]
        public string Room;

        [DataMember]
        public DateTime StartTime;

        [DataMember]
        public DateTime EndTime;

        [DataMember]
        public DayOfWeek DayOfWeek;

        [DataMember]
        public PowerPlannerSending.Schedule.Week ScheduleWeek;
    }

    [DataContract]
    public class GetAgendaResponse : PlainResponse
    {
        [DataMember]
        public IEnumerable<ListItemExamInfoWithType> Items;

        /// <summary>
        /// Note that this isn't even populated by the server and should simply be removed.
        /// </summary>
        [DataMember]
        public IEnumerable<BasicClassInfoWithSchedules> Classes;
    }

    [DataContract]
    public class GetAgendaLegacyResponse : PlainResponse
    {
        [DataContract]
        public class Class
        {
            [DataMember]
            public Guid Identifier;

            [DataMember]
            public string Name;

            [DataMember]
            public string Color;

            [DataMember]
            public List<Exam> Exams = new List<Exam>();

            [DataMember]
            public List<Homework> Homeworks = new List<Homework>();

            [DataMember]
            public List<Schedule> Schedules = new List<Schedule>();
        }

        [DataContract]
        public class Exam
        {
            [DataMember]
            public Guid Identifier;

            [DataMember]
            public string Name;

            [DataMember]
            public DateTime Date;
        }

        [DataContract]
        public class Homework : Exam
        {
            [DataMember]
            public double PercentComplete;
        }

        [DataContract]
        public class Schedule
        {
            [DataMember]
            public Guid Identifier;

            [DataMember]
            public string Room;

            [DataMember]
            public DateTime StartTime;

            [DataMember]
            public DateTime EndTime;

            [DataMember]
            public DayOfWeek DayOfWeek;

            [DataMember]
            public PowerPlannerSending.Schedule.Week ScheduleWeek;

            [DataMember]
            public double LocationLatitude;

            [DataMember]
            public double LocationLongitude;
        }

        [DataMember]
        public IEnumerable<Class> Classes;

        [DataMember]
        public DateTime WeekOneStartsOn;
    }

    [DataContract]
    public class GetSelectedSemesterIdRequest : WebsiteRequest
    {
        // Nothing
    }

    [DataContract]
    public class GetSelectedSemesterIdResponse : PlainResponse
    {
        [DataMember]
        public Guid? SelectedSemesterId;
    }

    [DataContract]
    public class SetSelectedSemesterIdRequest : WebsiteRequest
    {
        [DataMember]
        public Guid? SelectedSemesterId;
    }


    [DataContract]
    public class GetClassesAndScheduleRequest : WebsiteRequest
    {
        [DataMember]
        public Guid SemesterIdentifier;
    }

    [DataContract]
    public class GetClassesAndScheduleResponse : PlainResponse
    {
        [DataMember]
        public DateTime WeekOneStartsOn;

        [DataMember]
        public IEnumerable<BasicClassInfoWithSchedules> Classes { get; set; }
    }

    [DataContract]
    public class GetClassesScheduleAndMegaItemsResponse : PlainResponse
    {
        [DataMember]
        public DateTime WeekOneStartsOn;

        [DataMember]
        public IEnumerable<BasicClassInfo> Classes { get; set; }

        [DataMember]
        public IEnumerable<BasicScheduleInfo> Schedules { get; set; }

        [DataMember]
        public IEnumerable<ListItemMegaItem> MegaItems { get; set; }
    }



    [DataContract]
    public class GetYearsAndSemestersRequest : WebsiteRequest
    {
    }

    [DataContract]
    public class GetYearsAndSemestersResponse : PlainResponse
    {
        [DataContract]
        public class Year
        {
            [DataMember]
            public Guid Identifier;

            [DataMember]
            public string Name;

            /// <summary>
            /// Can be assumed that this will ALWAYS BE INITIALIZED
            /// </summary>
            [DataMember]
            public IEnumerable<Semester> Semesters;

            /// <summary>
            /// Not sent down. For sort purposes.
            /// </summary>
            public DateTime DateCreated;
        }

        [DataContract]
        public class Semester
        {
            [DataMember]
            public Guid Identifier;

            [DataMember]
            public string Name;

            /// <summary>
            /// Not sent down
            /// </summary>
            public Guid UpperIdentifier;

            //[DataMember]
            //public DateTime Start;

            //[DataMember]
            //public DateTime End;

            [DataMember]
            public IEnumerable<Class> Classes;

            /// <summary>
            /// Not sent down. For sort purposes.
            /// </summary>
            public DateTime DateCreated;
        }

        [DataContract]
        public class Class
        {
            [DataMember]
            public string Name;

            /// <summary>
            /// Not sent down
            /// </summary>
            public Guid UpperIdentifier;
        }

        [DataMember]
        public IEnumerable<Year> Years;
    }

    [DataContract]
    public class ChangeItemRequest : WebsiteRequest
    {
        [DataMember]
        public Guid Identifier;

        public static string GetImages(IEnumerable<string> images)
        {
            string answer = ListAdapter.ToString(images, StringParser.Parser);
            if (answer == null)
                return "";

            return answer;
        }
    }

    [DataContract]
    public class ChangeItemWithNameRequest : ChangeItemRequest
    {
        [Column("NVarChar600_1")]
        [DataMember]
        public string Name;
    }

    [DataContract]
    public class ChangeItemWithDetailsRequest : ChangeItemWithNameRequest
    {
        [Column("NVarChar4000_1")]
        [DataMember]
        public string Details;
    }

    [DataContract]
    public class ChangeItemWithImagesRequest : ChangeItemWithDetailsRequest
    {
        [Column("VarChar600_1")]
        public string RawImages
        {
            get { return GetImages(Images); }
        }

        [DataMember]
        public IEnumerable<string> Images;
    }

    [DataContract]
    public class ChangeHomeworkExamGradeRequest : ChangeItemWithImagesRequest
    {
        [Column("DateTime1")]
        [DataMember]
        public DateTime Date;
    }

    [DataContract]
    public class ChangeHomeworkExamRequest : ChangeHomeworkExamGradeRequest
    {
        [Column("UpperIdentifier")]
        [DataMember]
        public Guid ClassIdentifier;
    }

    [DataContract]
    public class ChangeWeightCategoryRequest : ChangeItemWithImagesRequest
    {
        [Column("Double1")]
        [DataMember]
        public double WeightValue;
    }

    [DataContract]
    public class ChangeGradeRequest : ChangeHomeworkExamGradeRequest
    {
        [Column("Double1")]
        [DataMember]
        public double GradeReceived;

        [Column("Double2")]
        [DataMember]
        public double GradeTotal;

        [Column("Double3")]
        [DataMember]
        public double IndividualWeight;
        
        [Column("UpperIdentifier")]
        [DataMember]
        public Guid WeightIdentifier;
    }

    [DataContract]
    public class ChangeHomeworkRequest : ChangeHomeworkExamRequest
    {
        //nothing extra
    }

    [DataContract]
    public class ChangeYearRequest : ChangeItemWithNameRequest
    {
        //nothing
    }

    [DataContract]
    public class ChangeSemesterRequest : ChangeItemWithNameRequest
    {
        [Column("DateTime1")]
        [DataMember]
        public DateTime Start;

        [Column("DateTime2")]
        [DataMember]
        public DateTime End;

        [Column("UpperIdentifier")]
        [DataMember]
        public Guid YearIdentifier;
    }

    [DataContract]
    public class ChangeExamRequest : ChangeHomeworkExamRequest
    {
        [Column("DateTime3")]
        [DataMember]
        public DateTime Reminder;
    }
}
