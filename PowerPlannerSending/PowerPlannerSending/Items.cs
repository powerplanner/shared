using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ToolsPortable;

namespace PowerPlannerSending
{
    public enum ItemType
    {
        Year = 0, Teacher = 1,

        Semester = 2,

        Class = 3, Task = 4,

        Homework = 5, Exam = 6, WeightCategory = 7, Schedule = 8,

        Grade = 9, TeacherUnderSchedule = 10,

        ClassAttribute = 11, ClassSubject = 12,
        ClassAttributeUnderClass = 13, ClassSubjectUnderClass = 14,

        MegaItem = 15
    }

    public class DateValues
    {
        public static bool IsUnassigned(DateTime date)
        {
            return date == UNASSIGNED || date == SqlDate.MinValue;
        }

        public static readonly DateTime UNASSIGNED = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly DateTime NO_DUE_DATE = new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        public static readonly DateTime LIFETIME_PREMIUM_ACCOUNT = DateTime.SpecifyKind(SqlDate.MaxValue, DateTimeKind.Utc);
    }


    /// <summary>
    /// Their main upper indexes reference <see cref="Class"/>, and their second indexes reference <see cref="ClassAttribute"/>. They can NOT change classes, but the reference to the ClassAttribute can be changed.
    /// </summary>
    [DataContract(Name = "ClassAttributeUnderClass", Namespace = "")]
    public class ClassAttributeUnderClass : BaseItemUnderTwo
    {
    }

    /// <summary>
    /// The first indexes reference <see cref="Class"/>, and the second indexes reference <see cref="ClassSubject"/>. They can NOT change classes, but the reference to the ClassSubject can be changed.
    /// </summary>
    [DataContract(Name = "ClassSubjectUnderClass", Namespace = "")]
    public class ClassSubjectUnderClass : BaseItemUnderTwo
    {
    }

    /// <summary>
    /// The base class, which contains the general properties that all the items will have.
    /// </summary>
    [KnownType(typeof(Class))]
    [KnownType(typeof(Exam))]
    [KnownType(typeof(Grade))]
    [KnownType(typeof(Homework))]
    [KnownType(typeof(Schedule))]
    [KnownType(typeof(Semester))]
    [KnownType(typeof(Task))]
    [KnownType(typeof(Teacher))]
    [KnownType(typeof(TeacherUnderSchedule))]
    [KnownType(typeof(WeightCategory))]
    [KnownType(typeof(Year))]
    [KnownType(typeof(ClassAttribute))]
    [KnownType(typeof(ClassSubject))]
    [KnownType(typeof(ClassAttributeUnderClass))]
    [KnownType(typeof(ClassSubjectUnderClass))]
    [DataContract]
    public abstract class BaseItem : IComparable<BaseItem>
    {
        public int Level()
        {
            if (this is Year || this is Teacher || this is ClassAttribute || this is ClassSubject)
                return 0;

            if (this is Semester)
                return 1;

            if (this is Task || this is Class)
                return 2;

            if (this is MegaItem)
            {
                switch ((this as MegaItem).MegaItemType)
                {
                    case MegaItemType.Holiday:
                    case MegaItemType.Task:
                    case MegaItemType.Event:
                        return 2;

                    default:
                        return 3;
                }
            }

            if (this is Homework || this is Exam || this is WeightCategory || this is Schedule || this is ClassAttributeUnderClass || this is ClassSubjectUnderClass)
                return 3;

            if (this is Grade || this is TeacherUnderSchedule)
                return 4;

            throw new NotImplementedException();
        }

        private ItemType? _itemType;
        public ItemType ItemType
        {
            get
            {
                if (_itemType == null)
                {
                    _itemType = (ItemType)Enum.Parse(typeof(ItemType), GetType().Name);
                }

                return _itemType.Value;
            }
        }

        public int CompareTo(BaseItem other)
        {
            return Level().CompareTo(other.Level());
        }

        /// <summary>
        /// A unique GUID identifier for the item. It's unique across all devices.
        /// </summary>
        [DataMember]
        public Guid Identifier;


        /// <summary>
        /// This is the unique identifer of the parent. If there's no parent (top item like year), this should be set to Guid.Empty (all 0's).
        /// </summary>
        [DataMember]
        public Guid UpperIdentifier { get; set; }

        /// <summary>
        /// The date/time, stored in UTC, when the item was updated.
        /// <br></br><br></br>
        /// For things like <see cref="Homework"/>, <see cref="Exam"/>, and <see cref="Task"/>, the Updated value should be used to help sort items. For example, if two homeworks are due on the same day, the one that was updated more recently should go last. That places the items that the user hasn't interacted with earlier in the list, since they probably don't remember those items as well.
        /// <br></br><br></br>
        /// <see cref="Grade"/> items should NOT use that sorting. Grades should simply be sorted by their date, disregarding any order of being updated.
        /// </summary>
        private DateTime _updated = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime Updated
        {
            get { return _updated; }

            //have to call toUtc() because Microsoft's JSON serializer will convert to local time when deserializing. When I personally set this value, my times are already UTC anyways, so it doesn't matter.
            set{ _updated = value.ToUniversalTime(); }
        }

        private DateTime _dateCreated = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { _dateCreated = value.ToUniversalTime();}
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        internal virtual void ApplyOffset(int offset)
        {
            Updated = applyOffset(Updated, offset);
            DateCreated = applyOffset(DateCreated, offset);
        }

        protected DateTime applyOffset(DateTime date, int offset)
        {
            return date.AddMilliseconds(offset);
        }
    }

    [DataContract]
    public abstract class BaseItemUnderTwo : BaseItem
    {
        [DataMember]
        public Guid SecondUpperIdentifier;
    }

    /// <summary>
    /// Items that store user-inputted names extend this class.
    /// </summary>
    [DataContract]
    public abstract class BaseItemWithName : BaseItem
    {
        /// <summary>
        /// User-inputted name of an item.
        /// <br></br><br></br>
        /// Maximum length is 600 characters. Unicode text (nvarchar). Can NOT be null. Does NOT allow line breaks.
        /// </summary>
        [DataMember]
        public string Name = "";
    }

    /// <summary>
    /// Items that store user-inputted details extend this class.
    /// </summary>
    [DataContract]
    public abstract class BaseItemWithDetails : BaseItemWithName
    {
        /// <summary>
        /// User-inputted details of an item. Could include hyperlinks, multiple lines, etc.
        /// <br></br><br></br>
        /// Maximum length is 4,000 characters. Unicode text (nvarchar). Can NOT be null. Allows line breaks.
        /// </summary>
        [DataMember]
        public string Details = "";
    }

    /// <summary>
    /// Items that can have overridden GPA and Credits extend this.
    /// </summary>
    [DataContract]
    public abstract class BaseItemWithOverriddenGPACredits : BaseItemWithImages
    {
        /// <summary>
        /// Overridden GPA that user explicitely set.
        /// </summary>
        [DataMember]
        public double OverriddenGPA = Grade.UNGRADED;

        /// <summary>
        /// Overridden credits that user explicitely set.
        /// </summary>
        [DataMember]
        public double OverriddenCredits = Grade.UNGRADED;
    }

    /// <summary>
    /// Items that can have user-taken images extend this.
    /// </summary>
    [DataContract]
    public abstract class BaseItemWithImages : BaseItemWithDetails
    {
        /// <summary>
        /// An array of image names, where each image name references unique images.
        /// <br></br><br></br>
        /// Max number of images per item is 15.
        /// <br></br><br></br>
        /// Format of each image name is "[DeviceId]-[UniqueNumber].[Format]" where<br></br>
        ///  DeviceId is the device id of the device that uploaded the image.<br></br>
        ///  UniqueNumber is a random number generated by the device. You must make sure that the number hasn't been generated by that device before.<br></br>
        ///  Format is something like "jpg", etc.
        /// <br></br><br></br>
        /// Default is empty array. Can NOT be null.
        /// </summary>
        [DataMember]
        public string[] ImageNames = new string[0];
    }

    /// <summary>
    /// This class helps generate the image name in the correct format.
    /// </summary>
    [DataContract]
    public class ImageIdentifier
    {
        /// <summary>
        /// The DeviceId of the device that uploaded the image.
        /// </summary>
        [DataMember]
        public long DeviceId;

        /// <summary>
        /// A unique number generated by the device, so that when DeviceId and UniqueNumber are combined, they form a primary key. When the device generates this, if it already generated one of the numbers before, it should generate a new one. A simple way of checking whether a number was already generated is to simply see if the image file exists on the device already.
        /// </summary>
        [DataMember]
        public int UniqueNumber;

        /// <summary>
        /// Should be something like "jpg", "png", whatever would go after a dot in a file name. Windows Phone currently just uses jpg images. Must be non-unicode (varchar).
        /// </summary>
        [DataMember]
        public string Format;

        /// <summary>
        /// The filename is produced as such: "[DeviceId]-[UniqueNumber].[Format]". Guaranteed to be non-unicode (varchar).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DeviceId + "-" + UniqueNumber + "." + Format;
        }
    }

    /// <summary>
    /// Homeworks, tasks, exams, and grades extend this, since they all have dates.
    /// </summary>
    [DataContract]
    public abstract class BaseHomeworkExamGrade : BaseItemWithImages
    {
        /// <summary>
        /// This stores the date that the user selected. It also optionally stores a start time.
        /// <br></br><br></br>
        /// Must be stored in UTC time. However, be careful and do not convert to UTC time, since that could change the date to a different day! Simply specify that the time is UTC, without changing the date or time.
        /// <br></br><br></br>
        /// The time component can represent the start time of an item. To represent no start time, leave the time component at 0:00:00.
        /// <br></br><br></br>
        /// If there's a start time, then the time component must be greater than 0 (one second greater would count). Time should be stored (but not converted) in UTC as discussed earlier. For example, 08:15:00 would represent a start time of 8:15 AM.
        /// <br></br><br></br>
        /// The time component currently isn't used in the Windows Phone or Windows 8 app.
        /// <br></br><br></br>
        /// "No due date" is represented by <see cref="DateValues.NO_DUE_DATE"/>. In that case, the time component is irrelevant.
        /// </summary>
        private DateTime _date = DateValues.NO_DUE_DATE;
        [DataMember]
        public DateTime Date
        {
            get { return _date; }
            set { _date = value.ToUniversalTime(); }
        }

        /// <summary>
        /// The grade (in points, NOT percent) that the user received.
        /// <br></br><br></br>
        /// UNGRADED is -50
        /// <br></br><br></br>
        /// Ungraded items are supported because users should be able to insert upcoming tests which aren't graded yet. They could then use a "What If?" mode to see what they would need to score on that ungraded test in order to get an A in the class. Ungraded items would simply be ignored from normal grade calculations.
        /// </summary>
        [DataMember]
        public double GradeReceived { get; set; } = Grade.UNGRADED;

        /// <summary>
        /// The total number of points that the item was out of.
        /// </summary>
        [DataMember]
        public double GradeTotal { get; set; } = 100;

        /// <summary>
        /// A boolean representing whether the grade has been dropped.
        /// <br></br><br></br>
        /// Default is false.
        /// <br></br><br></br>
        /// If this is true, then the grade should be ignored from class GPA calculation.
        /// <br></br><br></br>
        /// If this is false, then the grade should be included in class GPA calculation.
        /// </summary>
        [DataMember]
        public bool IsDropped { get; set; }

        /// <summary>
        /// Each grade could have an individual weight assigned to it.
        /// <br></br><br></br>
        /// Default value is 1.
        /// <br></br><br></br>
        /// When calculating grades, you would simply multiply both the GradeReceived and GradeTotal by the IndividualWeight in order to properly weight the item.
        /// </summary>
        [DataMember]
        public double IndividualWeight { get; set; } = 1;
    }

    /// <summary>
    /// <see cref="Homework"/>, <see cref="Exam"/> and <see cref="Task"/> extend this, since they can all have reminders and end times.
    /// </summary>
    [DataContract]
    public abstract class BaseHomeworkExam : BaseHomeworkExamGrade
    {
        /// <summary>
        /// Means that the item hasn't been assigned a weight category yet. Default value.
        /// </summary>
        public static readonly Guid WEIGHT_CATEGORY_UNASSIGNED = Guid.Empty;

        /// <summary>
        /// Means that the item will never be graded, and should be excluded from grades.
        /// </summary>
        public static readonly Guid WEIGHT_CATEGORY_EXCLUDED = new Guid("11111111-1111-1111-1111-111111111111");

        /// <summary>
        /// This is currently not used in Windows Phone and Windows 8 apps. However, in the future, it would let users specify an end time for an item, like an exam.
        /// <br></br><br></br>
        /// It would of course be stored as UTC.
        /// <br></br><br></br>
        /// Default unassigned value <see cref="DateValues.UNASSIGNED"/> (with any time component). Typically the assigned EndTime would be the same date as Date, but it technically could be multi-day and end on another day.
        /// </summary>
        private DateTime _endTime = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value.ToUniversalTime(); }
        }

        /// <summary>
        /// User can set a reminder notification for exams. It currently is not used in <see cref="Homework"/> for Windows Phone and Windows 8, but might be in the future. It's used for <see cref="Exam"/>.
        /// <br></br><br></br>
        /// Must be stored as UTC (but not converted, simply marked as UTC). That way, a reminder set for 8 PM will always go off at 8 PM, no matter what time zone you travel to.
        /// <br></br><br></br>
        /// No reminder is the value of <see cref="DateValues.UNASSIGNED"/>
        /// </summary>
        private DateTime _reminder = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime Reminder
        {
            get { return _reminder; }
            set { _reminder = value.ToUniversalTime(); }
        }

        [DataMember]
        public Guid WeightCategoryIdentifier { get; set; }
    }

    /// <summary>
    /// <see cref="Homework"/> and <see cref="Task"/> extend this class.
    /// </summary>
    [DataContract]
    public abstract class BaseHomework : BaseHomeworkExam
    {
        /// <summary>
        /// A value between 0 and 1, inclusive, representing how complete a homework/task is.
        /// <br></br><br></br>
        /// 0 represents incomplete.<br></br>
        /// 1 represents complete.
        /// <br></br><br></br>
        /// On platforms that do not support percent complete (and only support complete/incomplete), anything not equal to 1 will be considered incomplete. For example, 0.99 would be incomplete. Only 1.0 will be considered complete. Windows Phone and Windows 8 apps currently do not support percent complete.
        /// </summary>
        [DataMember]
        public double PercentComplete;
    }

    /// <summary>
    /// Homeworks go under <see cref="Class"/> objects.
    /// <br></br><br></br>
    /// They are allowed to switch parents (a homework assignment could be edited so that it switches from Math to Spanish class. The upper indexes would simply change).
    /// </summary>
    [DataContract(Name="Homework", Namespace="")]
    public class Homework : BaseHomework
    {
        //nothing extra
    }

    public enum MegaItemType
    {
        // ORDER MATTERS, don't rearrange
        Homework,
        Exam,
        Holiday,
        Task,
        Event,
        ClassTime
    }

    [DataContract(Name="MegaItem", Namespace="")]
    public class MegaItem : BaseHomeworkExamGrade
    {
        [DataMember]
        public MegaItemType MegaItemType { get; set; }

        /// <summary>
        /// Means that the item hasn't been assigned a weight category yet. Default value.
        /// </summary>
        public static readonly Guid WEIGHT_CATEGORY_UNASSIGNED = Guid.Empty;

        /// <summary>
        /// Means that the item will never be graded, and should be excluded from grades.
        /// </summary>
        public static readonly Guid WEIGHT_CATEGORY_EXCLUDED = new Guid("11111111-1111-1111-1111-111111111111");

        /// <summary>
        /// This is currently not used in Windows Phone and Windows 8 apps. However, in the future, it would let users specify an end time for an item, like an exam.
        /// <br></br><br></br>
        /// It would of course be stored as UTC.
        /// <br></br><br></br>
        /// Default unassigned value <see cref="DateValues.UNASSIGNED"/> (with any time component). Typically the assigned EndTime would be the same date as Date, but it technically could be multi-day and end on another day.
        /// </summary>
        private DateTime _endTime = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value.ToUniversalTime(); }
        }

        /// <summary>
        /// User can set a reminder notification for exams. It currently is not used in <see cref="Homework"/> for Windows Phone and Windows 8, but might be in the future. It's used for <see cref="Exam"/>.
        /// <br></br><br></br>
        /// Must be stored as UTC (but not converted, simply marked as UTC). That way, a reminder set for 8 PM will always go off at 8 PM, no matter what time zone you travel to.
        /// <br></br><br></br>
        /// No reminder is the value of <see cref="DateValues.UNASSIGNED"/>
        /// </summary>
        private DateTime _reminder = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime Reminder
        {
            get { return _reminder; }
            set { _reminder = value.ToUniversalTime(); }
        }

        [DataMember]
        public Guid WeightCategoryIdentifier { get; set; }

        /// <summary>
        /// A value between 0 and 1, inclusive, representing how complete a homework/task is.
        /// <br></br><br></br>
        /// 0 represents incomplete.<br></br>
        /// 1 represents complete.
        /// <br></br><br></br>
        /// On platforms that do not support percent complete (and only support complete/incomplete), anything not equal to 1 will be considered incomplete. For example, 0.99 would be incomplete. Only 1.0 will be considered complete. Windows Phone and Windows 8 apps currently do not support percent complete.
        /// </summary>
        [DataMember]
        public double PercentComplete;

        public BaseItem DownConvert()
        {
            BaseHomeworkExam item;
            if (MegaItemType == MegaItemType.Homework)
            {
                item = new Homework()
                {
                    PercentComplete = PercentComplete
                };
            }
            else if (MegaItemType == MegaItemType.Exam)
            {
                item = new Exam();
            }
            else
            {
                return null;
            }

            item.Date = Date;
            item.DateCreated = DateCreated;
            item.Details = Details;
            item.EndTime = EndTime;
            item.GradeReceived = GradeReceived;
            item.GradeTotal = GradeTotal;
            item.Identifier = Identifier;
            item.ImageNames = ImageNames;
            item.IndividualWeight = IndividualWeight;
            item.IsDropped = IsDropped;
            item.Name = Name;
            item.Reminder = Reminder;
            item.Updated = Updated;
            item.UpperIdentifier = UpperIdentifier;
            item.WeightCategoryIdentifier = WeightCategoryIdentifier;

            return item;
        }
    }

    /// <summary>
    /// This currently isn't supported in the Windows Phone and Windows 8 apps. When it is supported, tasks would go under <see cref="Semester"/> objects, so that users could have general tasks like "Get more paper".
    /// <br></br><br></br>
    /// They would NOT be allowed to switch parents.
    /// </summary>
    [DataContract(Name="Task", Namespace="")]
    public class Task : BaseHomework
    {
        //nothing extra
    }

    /// <summary>
    /// Exams go under <see cref="Class"/> objects.
    /// <br></br><br></br>
    /// They are allowed to switch parents (an exam could be edited so that it switches from Math to Spanish class. The upper indexes would simply change).
    /// </summary>
    [DataContract(Name="Exam", Namespace="")]
    public class Exam : BaseHomeworkExam
    {
        //nothing extra
    }

    /// <summary>
    /// Classes go under <see cref="Semester"/> objects.
    /// <br></br><br></br>
    /// They are allowed to switch parents (a user could switch a class to a different semester, which would have to move all their homeworks, grades, etc from that class into the new semester too. However, the only thing that changes is the upper indexes of the class!).
    /// <br></br><br></br>
    /// Children are <see cref="Homework"/>, <see cref="Exam"/>, <see cref="Schedule"/>, <see cref="ClassAttribute"/>, and <see cref="WeightCategory"/>. There must always be at least one <see cref="WeightCategory"/> in each class.
    /// </summary>
    [DataContract(Name="Class", Namespace="")]
    public class Class : BaseItemWithImages
    {
        /// <summary>
        /// Exposed as a percentage, so 0.6
        /// </summary>
        public const double DefaultPassingGrade = 0.6;

        /// <summary>
        /// A course number, like "127A".
        /// </summary>
        [DataMember]
        public string CourseNumber = "";
        /// <summary>
        /// The number of credit hours a class has.
        /// <br></br><br></br>
        /// Default value of NONE credits is -1.
        /// <br></br><br></br>
        /// If all of the classes have NONE credits, then every class should be weighted equally when computing the GPA for the semester (and the NONE credits would transfer up to the semester too).
        /// <br></br><br></br>
        /// If some classes have NONE credits and others have a different value, then the classes with NONE credits will simply be ignored from the GPA calculation.
        /// </summary>
        [DataMember]
        public double Credits;

        /// <summary>
        /// A boolean representing whether we should average grade totals (instead of sum).
        /// <br></br><br></br>
        /// Default value is false.
        /// <br></br><br></br>
        /// If this is false, then the class grade should be calculated by summing the total number of points and dividing by the total number of possible points.
        /// <br></br><br></br>
        /// If this is true, then the class grade should be calculated by averaging every percentage of every grade.
        /// </summary>
        [DataMember]
        public bool ShouldAverageGradeTotals;

        /// <summary>
        /// A boolean representing whether we should round when deciding the final letter grade and GPA.
        /// <br></br><br></br>
        /// Default value is true.
        /// <br></br><br></br>
        /// If this is true, then if the user has a 89.6%, you must round up to a 90% when deciding what GPA/grade letter they got. Therefore, they would receive an A.
        /// <br></br><br></br>
        /// If this is false, then a user's 89.6% will give them a B in the class.
        /// </summary>
        [DataMember]
        public bool DoesRoundGradesUp;

        /// <summary>
        /// Classes are allowed to have user-selected colors, which will apply to their children homework, exams, and grades.
        /// <br></br><br></br>
        /// Default value is TODO. Format stored as 3 bytes { R, G, B }. A list is used instead of array cause JSON.net would serialize a byte array as a base64 string.
        /// </summary>
        [DataMember]
        public string Color = "#1BA1E2";

        /// <summary>
        /// The Position value can be used so the user can optionally sort the order of their classes.
        /// <br></br><br></br>
        /// Value must be between 0 - 255, inclusive. Default is 0.
        /// <br></br><br></br>
        /// Windows Phone and Windows 8 currently do not support editing positions. The class positions would be reflected properly, but the user wouldn't be able to re-order their classes on these platforms.
        /// </summary>
        [DataMember]
        public byte Position;

        /// <summary>
        /// Determines the class' GPA type, which affects how the overall GPA is calculated.
        /// </summary>
        [DataMember]
        public GpaType GpaType;

        /// <summary>
        /// Determines what the passing grade is for the class. Only taken into account if <see cref="GpaType"/> is set to <see cref="GpaType.PassFail"/>.
        /// </summary>
        [DataMember]
        public double PassingGrade;

        /// <summary>
        /// This is a list of the grade scales for the class.
        /// <br></br><br></br>
        /// Default value is NULL, meaning the app should use the traditional American grade scale.
        /// </summary>
        [DataMember]
        public GradeScale[] GradeScales;


        /// <summary>
        /// Overridden GPA that user explicitely set.
        /// </summary>
        [DataMember]
        public double OverriddenGPA = Grade.UNGRADED;

        /// <summary>
        /// Overridden grade that user explicitely set.
        /// </summary>
        [DataMember]
        public double OverriddenGrade = Grade.UNGRADED;

        /// <summary>
        /// The date that the class starts on within the semester. If left unassigned, starts on the first day of the semester.
        /// </summary>
        [DataMember]
        public DateTime StartDate = SqlDate.MinValue;

        /// <summary>
        /// The date that the class ends within the semester. If left unassigned, ends on the last day of the semester.
        /// </summary>
        [DataMember]
        public DateTime EndDate = SqlDate.MinValue;
    }

    /// <summary>
    /// Determines how the class affects overall GPA.
    /// </summary>
    public enum GpaType
    {
        /// <summary>
        /// Both credits and GPA count
        /// </summary>
        Standard = 0,

        /// <summary>
        /// GPA is completely ignored. If passed, credits count. If failed, the class doesn't impact anything.
        /// </summary>
        PassFail = 1
    }

    /// <summary>
    /// Grades go under <see cref="WeightCategory"/> objects.
    /// <br></br><br></br>
    /// They are allowed to switch parents (a grade could be edited to move from the Essays to the Midterm weight category). However, grades can never switch between different classes. They can only switch between different weight categories inside a single class.
    /// </summary>
    [DataContract(Name="Grade", Namespace="")]
    public class Grade : BaseHomeworkExamGrade
    {
        public static readonly double UNGRADED = -50;

        public const double NO_CREDITS = -1;
    }

    /// <summary>
    /// Schedules go under <see cref="Class"/> objects.
    /// <br></br><br></br>
    /// They are NOT allowed to switch parents.
    /// <br></br><br></br>
    /// Schedules also have lists of pointers to teachers beneath them (<see cref="TeacherUnderSchedule"/>), so that multiple teachers could be at a single class time.
    /// </summary>
    [DataContract(Name="Schedule", Namespace="")]
    public class Schedule : BaseItemWithImages
    {
        /// <summary>
        /// The type that the schedule is, like a normal class, or lab hours.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// The default type of class, like a lecture.
            /// </summary>
            Normal = 0,

            /// <summary>
            /// Office hours for a class.
            /// </summary>
            OfficeHours = 1,

            /// <summary>
            /// Lab or discussion.
            /// </summary>
            Lab = 2
        }

        /// <summary>
        /// For use with a two-week rotating schedule, to specify what week a schedule occurs on.
        /// </summary>
        public enum Week
        {
            /// <summary>
            /// Schedule repeats on Week 1/Week A
            /// </summary>
            WeekOne = 1, // 01

            /// <summary>
            /// Schedule repeats on Week 2/Week B
            /// </summary>
            WeekTwo = 2, // 10

            /// <summary>
            /// Schedule repeats on both Week 1 and Week 2 (if the user has the same schedule every week, this option should be selected).
            /// </summary>
            BothWeeks = 3 //11
        };

        /// <summary>
        /// The day of the week that the schedule occurs on
        /// </summary>
        [DataMember]
        public DayOfWeek DayOfWeek;

        /// <summary>
        /// The time that the schedule starts. Only the time component matters.
        /// </summary>
        private DateTime _startTime = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value.ToUniversalTime(); }
        }

        /// <summary>
        /// The time that the schedule ends. See StartTime for more info.
        /// </summary>
        private DateTime _endTime = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value.ToUniversalTime(); }
        }

        /// <summary>
        /// The room where the schedule takes place.
        /// <br></br><br></br>
        /// Max length 600. Unicode text (nvarchar). Can NOT be null.
        /// </summary>
        [DataMember]
        public string Room = "";

        /// <summary>
        /// The week that the potentially two-week rotating schedule occurs on.
        /// </summary>
        [DataMember]
        public Week ScheduleWeek;

        /// <summary>
        /// The type of the schedule, like a normal class, office hours, or lab.
        /// </summary>
        [DataMember]
        public Type ScheduleType;

        [DataMember]
        public double LocationLatitude;

        [DataMember]
        public double LocationLongitude;
    }

    /// <summary>
    /// This goes under <see cref="Schedule"/>.
    /// <br></br><br></br>
    /// It simply acts as a pointer to the actual stored <see cref="Teacher"/> value.
    /// </summary>
    [DataContract(Name="TeacherUnderSchedule", Namespace="")]
    public class TeacherUnderSchedule : BaseItemUnderTwo
    {
    }

    /// <summary>
    /// Semesters go under <see cref="Year"/> objects.
    /// <br></br><br></br>
    /// They can NOT switch parents.
    /// <br></br><br></br>
    /// Children of semesters are <see cref="Class"/> and <see cref="Task"/>, although <see cref="Task"/> current isn't used in the Windows Phone and Windows 8 apps.
    /// <br></br><br></br>
    /// Also, the Details and ImageNames properties currently aren't used by the Windows Phone and Windows 8 apps.
    /// </summary>
    [DataContract(Name="Semester", Namespace="")]
    public class Semester : BaseItemWithOverriddenGPACredits
    {
        /// <summary>
        /// The date that the semester starts.
        /// <br></br><br></br>
        /// Should be stored as UTC, but not converted to UTC.
        /// <br></br><br></br>
        /// Default unassigned value is 1/1/1970.
        /// <br></br><br></br>
        /// This property currently isn't used by the Windows Phone and Windows 8 apps.
        /// </summary>
        private DateTime _start = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime Start
        {
            get { return _start; }
            set { _start = value.ToUniversalTime(); }
        }

        /// <summary>
        /// The date that the semester ends.
        /// <br></br><br></br>
        /// Should be stored as UTC, but not converted to UTC.
        /// <br></br><br></br>
        /// Default unassigned value is 1/1/1970.
        /// <br></br><br></br>
        /// This property currently isn't used by Windows Phone and Windows 8 apps.
        /// </summary>
        private DateTime _end = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime End
        {
            get { return _end; }
            set { _end = value.ToUniversalTime(); }
        }
    }

    /// <summary>
    /// Teachers are up at the top just like <see cref="Year"/>, they don't have a parent. Users can have a list of teachers persistent throughout their school career. This is because teachers might be reused throughout classes. For example, Professor McCann teaches CSC 245 and CSC 345. There would be no need to re-create his info, like his name, phone number, email address, etc.
    /// 
    /// ImageNames property currently isn't used by Windows Phone and Windows 8 apps.
    /// </summary>
    [DataContract(Name="Teacher", Namespace="")]
    public class Teacher : BaseItemWithImages
    {
        /// <summary>
        /// An array of their phone numbers. Each teacher could have multiple phone numbers.
        /// <br></br><br></br>
        /// Default is an empty array. It can NOT be null.
        /// </summary>
        [DataMember]
        public PhoneNumber[] PhoneNumbers;

        /// <summary>
        /// An array of their email addresses.
        /// <br></br><br></br>
        /// Default is empty array. It can NOT be null.
        /// </summary>
        [DataMember]
        public EmailAddress[] EmailAddresses;

        /// <summary>
        /// An array of their physical addresses.
        /// <br></br><br></br>
        /// Default is empty array. Can NOT be null.
        /// </summary>
        [DataMember]
        public PostalAddress[] PostalAddresses;

        /// <summary>
        /// An array of their office locations. Each location is simply a string, like "Gould Simpson, 901".
        /// <br></br><br></br>
        /// Default is empty array. Can NOT be null.
        /// </summary>
        [DataMember]
        public string[] OfficeLocations;
    }

    /// <summary>
    /// An item for conveying email address information.
    /// </summary>
    [DataContract(Name="EmailAddress", Namespace="")]
    public class EmailAddress : IEquatable<EmailAddress>
    {
        /// <summary>
        /// The type of their email.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// A work email.
            /// </summary>
            Work = 0,

            /// <summary>
            /// A personal email
            /// </summary>
            Personal = 1,

            /// <summary>
            /// Any other type of email
            /// </summary>
            Other = 2
        }

        /// <summary>
        /// Their email address.
        /// <br></br><br></br>
        /// No length restriction.
        /// </summary>
        [DataMember]
        public string Email;

        /// <summary>
        /// The type that the email is.
        /// </summary>
        [DataMember]
        public Type EmailType;

        public bool Equals(EmailAddress other)
        {
            return Email.Equals(other.Email) && EmailType == other.EmailType;
        }
    }

    /// <summary>
    /// An item conveying postal address information.
    /// </summary>
    [DataContract(Name="PostalAddress", Namespace="")]
    public class PostalAddress : IEquatable<PostalAddress>
    {
        /// <summary>
        /// Type of a postal address.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// A work address.
            /// </summary>
            Work = 0,

            /// <summary>
            /// A home address.
            /// </summary>
            Home = 1,

            /// <summary>
            /// A post office address.
            /// </summary>
            PostOfficeBox = 2,

            /// <summary>
            /// An address to their office.
            /// </summary>
            Other = 3
        }

        /// <summary>
        /// The street of their postal address.
        /// <br></br><br></br>
        /// Can not contain line breaks.
        /// </summary>
        [DataMember]
        public string StreetLine1;

        /// <summary>
        /// Extra street line if necessary.
        /// <br></br><br></br>
        /// Can not contain newlines.
        /// </summary>
        [DataMember]
        public string StreetLine2;

        /// <summary>
        /// The city
        /// </summary>
        [DataMember]
        public string City;

        /// <summary>
        /// The state
        /// </summary>
        [DataMember]
        public string State;

        /// <summary>
        /// The zip code
        /// </summary>
        [DataMember]
        public string ZIP;

        /// <summary>
        /// The country
        /// </summary>
        [DataMember]
        public string Country;

        /// <summary>
        /// The type of address
        /// </summary>
        [DataMember]
        public Type AddressType;

        public bool Equals(PostalAddress other)
        {
            return StreetLine1.Equals(other.StreetLine1) &&
                StreetLine2.Equals(other.StreetLine2) &&
                City.Equals(other.City) &&
                State.Equals(other.State) &&
                ZIP.Equals(other.ZIP) &&
                Country.Equals(other.Country) &&
                AddressType == other.AddressType;
        }
    }

    /// <summary>
    /// An item conveying phone number information.
    /// </summary>
    [DataContract(Name="PhoneNumber", Namespace="")]
    public class PhoneNumber : IEquatable<PhoneNumber>
    {
        /// <summary>
        /// The type of a phone number.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// An office/work phone
            /// </summary>
            Office = 0,

            /// <summary>
            /// A home phone
            /// </summary>
            Home = 1,

            /// <summary>
            /// A cell phone
            /// </summary>
            Cell = 2,

            /// <summary>
            /// A fax number
            /// </summary>
            Fax = 3,

            /// <summary>
            /// Any other type
            /// </summary>
            Other = 3
        }

        /// <summary>
        /// Their actual phone number, stored exactly how the user inputted it.
        /// <br></br><br></br>
        /// Cannot contain line breaks.
        /// </summary>
        [DataMember]
        public string Number;

        /// <summary>
        /// Any potential extension info, stored exactly how the user inputted it.
        /// <br></br><br></br>
        /// Cannot contain line breaks.
        /// </summary>
        [DataMember]
        public string Extension;

        /// <summary>
        /// The type of the phone number.
        /// </summary>
        [DataMember]
        public Type PhoneType;


        public bool Equals(PhoneNumber other)
        {
            return Number.Equals(other.Number) && Extension.Equals(other.Extension) && PhoneType == other.PhoneType;
        }
    }

    /// <summary>
    /// Weight categories go under <see cref="Class"/> objects.
    /// <br></br><br></br>
    /// They can NOT change parents.
    /// <br></br><br></br>
    /// These let you have categories for Homework (20%), Exams (40%), Final (30%), Participation (10%). They all must add up to 100% for each class.
    /// <br></br><br></br>
    /// Default category simply has the Name = "All Grades", and WeightPercent = 1;
    /// <br></br><br></br>
    /// The ImageNames property currently isn't used by Windows Phone or Windows 8 apps.
    /// <br></br><br></br>
    /// Children are <see cref="Grade"/> items.
    /// </summary>
    [DataContract(Name="WeightCategory", Namespace="")]
    public class WeightCategory : BaseItemWithImages
    {
        /// <summary>
        /// The weight percent (represented from 0 - 100) of the category.
        /// <br></br><br></br>
        /// Weight values across a class don't need to add up to 100. They could be 300 total, for example.
        /// </summary>
        [DataMember]
        public double WeightValue;
    }

    /// <summary>
    /// Years are at the top of the chain, they do not have a parent. Thus, their upper indexes must be -1.
    /// <br></br><br></br>
    /// ImageNames and Details property currently isn't used by Windows Phone and Windows 8 apps.
    /// <br></br><br></br>
    /// Children are <see cref="Semester"/> items.
    /// </summary>
    [DataContract(Name="Year", Namespace="")]
    public class Year : BaseItemWithOverriddenGPACredits
    {
    }

    /// <summary>
    /// An object that stores a single entry in a grade scale.
    /// </summary>
    [DataContract(Name="GradeScale", Namespace="")]
    public class GradeScale : BindableBase, IComparable, IComparable<GradeScale>, IEquatable<GradeScale>
    {
        public GradeScale()
        {

        }

        public GradeScale(double startGrade, double gpa)
        {
            StartGrade = startGrade;
            GPA = gpa;
        }

        public static GradeScale[] GenerateDefaultScaleWithoutLetters()
        {
            return new GradeScale[]
            {
                new GradeScale() { StartGrade = 90, GPA = 4 },
                new GradeScale() { StartGrade = 80, GPA = 3 },
                new GradeScale() { StartGrade = 70, GPA = 2 },
                new GradeScale() { StartGrade = 60, GPA = 1 },
                new GradeScale() { StartGrade = 0, GPA = 0 }
            };
        }

        //public static GradeScale[] GenerateDefaultScale()
        //{
        //    return new GradeScale[]
        //    {
        //        new GradeScale() { GPA = 4, LetterGrade = "A+", StartGrade = 97 },
        //        new GradeScale() { GPA = 4, LetterGrade = "A", StartGrade = 93 },
        //        new GradeScale() { GPA = 4, LetterGrade = "A-", StartGrade = 90 },
        //        new GradeScale() { GPA = 3, LetterGrade = "B+", StartGrade = 87 },
        //        new GradeScale() { GPA = 3, LetterGrade = "B", StartGrade = 83 },
        //        new GradeScale() { GPA = 3, LetterGrade = "B-", StartGrade = 80 },
        //        new GradeScale() { GPA = 2, LetterGrade = "C+", StartGrade = 77 },
        //        new GradeScale() { GPA = 2, LetterGrade = "C", StartGrade = 73 },
        //        new GradeScale() { GPA = 2, LetterGrade = "C-", StartGrade = 70 },
        //        new GradeScale() { GPA = 1, LetterGrade = "D+", StartGrade = 67 },
        //        new GradeScale() { GPA = 1, LetterGrade = "D", StartGrade = 63 },
        //        new GradeScale() { GPA = 1, LetterGrade = "D-", StartGrade = 60 },
        //        new GradeScale() { GPA = 0, LetterGrade = "F", StartGrade = 0 }
        //    };
        //}


        public static GradeScale[] GenerateElevenPointScale()
        {
            return new GradeScale[]
            {
                new GradeScale() { GPA = 11, StartGrade = 93 },
                new GradeScale() { GPA = 10, StartGrade = 90 },
                new GradeScale() { StartGrade = 87, GPA = 9 },
                new GradeScale() { GPA = 8, StartGrade = 83 },
                new GradeScale() { GPA = 7, StartGrade = 80 },
                new GradeScale() { GPA = 6, StartGrade = 77 },
                new GradeScale() { GPA = 5, StartGrade = 73 },
                new GradeScale() { GPA = 4, StartGrade = 70 },
                new GradeScale() { GPA = 3, StartGrade = 67 },
                new GradeScale() { GPA = 2, StartGrade = 63 },
                new GradeScale() { GPA = 1, StartGrade = 60 },
                new GradeScale() { GPA = 0, StartGrade = 0 }
            };
        }

        public static GradeScale[] GenerateTwelvePointScale()
        {
            return new GradeScale[]
            {
                new GradeScale() { GPA = 12, StartGrade = 97 },
                new GradeScale() { GPA = 11, StartGrade = 93 },
                new GradeScale() { GPA = 10, StartGrade = 90 },
                new GradeScale() { StartGrade = 87, GPA = 9 },
                new GradeScale() { GPA = 8, StartGrade = 83 },
                new GradeScale() { GPA = 7, StartGrade = 80 },
                new GradeScale() { GPA = 6, StartGrade = 77 },
                new GradeScale() { GPA = 5, StartGrade = 73 },
                new GradeScale() { GPA = 4, StartGrade = 70 },
                new GradeScale() { GPA = 3, StartGrade = 67 },
                new GradeScale() { GPA = 2, StartGrade = 63 },
                new GradeScale() { GPA = 1, StartGrade = 60 },
                new GradeScale() { GPA = 0, StartGrade = 0 }
            };
        }

        public static GradeScale[] GenerateMexico100PointScale()
        {
            GradeScale[] answer = new GradeScale[100];

            for (int i = 0; i < 100; i++)
                answer[i] = new GradeScale(100 - i, 100 - i);

            return answer;
        }

        public static GradeScale[] GenerateMexico10PointScale()
        {
            GradeScale[] answer = new GradeScale[100];

            for (int i = 0; i < 100; i++)
                answer[i] = new GradeScale(100 - i, (100 - i) / 10.0);

            return answer;
        }

        private double _startGrade;
        /// <summary>
        /// The start grade (inclusive).
        /// <br></br><br></br>
        /// Typical range is 0 - 100.
        /// <br></br><br></br>
        /// For example, this could be 97.
        /// </summary>
        [DataMember]
        public double StartGrade
        {
            get { return _startGrade; }
            set { SetProperty(ref _startGrade, value, "StartGrade"); }
        }

        private double _gpa;
        /// <summary>
        /// The GPA associated with this range.
        /// <br></br><br></br>
        /// Typical range is 0 - 4.
        /// <br></br><br></br>
        /// For example, this could be 4.0.
        /// </summary>
        [DataMember]
        public double GPA
        {
            get { return _gpa; }
            set { SetProperty(ref _gpa, value, "GPA"); }
        }

        //private string _letterGrade;
        ///// <summary>
        ///// The grade letter associated with this range.
        ///// <br></br><br></br>
        ///// Typical values are "A", "B", "C", etc.
        ///// <br></br><br></br>
        ///// For example, this could be "A+".
        ///// </summary>
        //[DataMember]
        //public string LetterGrade
        //{
        //    get { return _letterGrade; }
        //    set { SetProperty(ref _letterGrade, value, "LetterGrade"); }
        //}

        public int CompareTo(GradeScale other)
        {
            if (this.StartGrade > other.StartGrade)
                return -1;

            return 1;
        }

        public int CompareTo(object obj)
        {
            if (obj is GradeScale)
                return CompareTo(obj as GradeScale);

            return 0;
        }

        /// <summary>
        /// Checks that the LetterGrade, StartGrade, and GPA are the same
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(GradeScale other)
        {
            //otherwise, either both are null or both are initialized
            return (StartGrade == other.StartGrade && GPA == other.GPA);
        }
    }

    /// <summary>
    /// ClassAttribute values go at the TOP like <see cref="Year"/>. They are linked to <see cref="Class"/> through <see cref="ClassAttributeUnderClass"/>. Classes can have multiple class attributes, like "Honors Course", "TRAD 160", etc. The "Name" value would contain that value.
    /// </summary>
    [DataContract(Name = "ClassAttribute", Namespace="")]
    public class ClassAttribute : BaseItemWithName
    {
    }

    /// <summary>
    /// ClassSubject values go at the TOP like <see cref="Year"/>. They are linked to <see cref="Class"/> through <see cref="ClassSubjectUnderClass"/>. Classes can have multiple class subjects, like "CSC" and "ECE", since some courses can be interdisciplinary. "Name" would contain "CSC" and "Description" would contain "Computer Science".
    /// </summary>
    [DataContract(Name="ClassSubject", Namespace="")]
    public class ClassSubject : BaseItemWithDetails
    {
    }

    [DataContract(Name="YearWithChildren", Namespace="")]
    public class YearWithChildren : Year
    {
        [DataMember]
        public List<Semester> Semesters;
    }

    /// <summary>
    /// See <see cref="Class"/> for info about the properties inside each class. This object also has the arrays of homework, exams, and schedules for the given class.
    /// </summary>
    [DataContract(Name="ClassWithChildren", Namespace="")]
    public class ClassWithChildren : Class
    {
        /// <summary>
        /// An array of the homework belonging to this class.
        /// </summary>
        [DataMember]
        public IEnumerable<Homework> Homework;

        /// <summary>
        /// An array of the exams belonging to this class.
        /// </summary>
        [DataMember]
        public IEnumerable<Exam> Exams;

        /// <summary>
        /// An array of the schedules belonging to this class.
        /// </summary>
        [DataMember]
        public IEnumerable<Schedule> Schedules;
    }

    [DataContract(Name="ScheduleWithChildren", Namespace="")]
    public class ScheduleWithChildren : Schedule
    {
        [DataMember]
        public List<Teacher> Teachers;
    }
}
