using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ToolsPortable;

namespace PowerPlannerSending
{
    [DataContract]
    public class PartialLoginRequest
    {
        [DataMember]
        public virtual LoginCredentials Login { get; set; }
    }

    [DataContract]
    public class CreateAccountRequest
    {
        /// <summary>
        /// Max length is 50, no min length (must not be empty though).
        /// </summary>
        [DataMember]
        public string Username;

        [DataMember]
        public string Password;

        [DataMember]
        public string Email;

        [DataMember]
        public bool AddDevice;
    }

    [DataContract]
    public class CreateAccountResponse : PlainResponse
    {
        public static readonly string USERNAME_TAKEN = "This username already exists. Pick a different username. If this is your account, then check your password.";

        [DataMember]
        public long AccountId;
        [DataMember]
        public bool Created;
        [DataMember]
        public bool ExistsButCredentialsMatched;
        [DataMember]
        public int DeviceId;

        /// <summary>
        /// The hashed password, web version uses this
        /// </summary>
        [DataMember]
        public string Session;
    }

    [DataContract]
    public class AddDeviceRequest
    {
        [DataMember]
        public string Username;

        [DataMember]
        public string Password;
    }

    [DataContract]
    public class AddDeviceResponse : PlainResponse
    {
        [DataMember]
        public long AccountId;
        [DataMember]
        public int DeviceId;
        [DataMember]
        public string Session;
        [DataMember]
        public SyncedSettings Settings;
        /// <summary>
        /// You must store this on the device and send it up with each sync. This allows the server to only send down a new grade scale if it actually changed.
        /// </summary>
        [DataMember]
        public long DefaultGradeScaleIndex;
    }

    [DataContract]
    public class DeleteAccountRequest : PartialLoginRequest
    {
    }

    [DataContract]
    public class DeleteAccountResponse : PlainResponse
    {
    }

    [DataContract]
    public class DeleteDevicesRequest : PartialLoginRequest
    {
        [DataMember]
        public List<int> DeviceIdsToDelete;
    }

    [DataContract]
    public class DeleteDevicesResponse : PlainResponse
    {
    }

    [DataContract]
    public class ResetPasswordRequest
    {
        /// <summary>
        /// The username the user forgot his password for
        /// </summary>
        [DataMember]
        public string Username;

        /// <summary>
        /// The email associated with the user's account. Helps reduce spam, so that someone doesn't try resetting random usernames.
        /// </summary>
        [DataMember]
        public string Email;
    }

    [DataContract]
    public class ResetPasswordResponse : PlainResponse
    {
        /// <summary>
        /// This will contain something like "An email has been sent to xxx@outlook.com with a link to reset your password."
        /// </summary>
        [DataMember]
        public string Message;
    }

    /// <summary>
    /// This request retrieves the online-stored email address for their account. Useful for if you're showing the username what their email linked to their account currently is.
    /// </summary>
    [DataContract]
    public class GetEmailRequest : PartialLoginRequest
    {
    }

    [DataContract]
    public class GetEmailResponse : PlainResponse
    {
        [DataMember]
        public string Email;

        /// <summary>
        /// If true, means that the user's email has been verified through the confirmation email. Otherwise, means the user hasn't yet verified their email.
        /// </summary>
        [DataMember]
        public bool EmailVerified;
    }

    [DataContract]
    public class ForgotUsernameRequest
    {
        [DataMember]
        public string Email;
    }

    [DataContract]
    public class ForgotUsernameResponse : PlainResponse
    {
        [DataMember]
        public List<string> Usernames;
    }

    [DataContract]
    public class ChangeEmailRequest : PartialLoginRequest
    {
        [DataMember]
        public string NewEmail;
    }

    [DataContract]
    public class ChangeEmailResponse : PlainResponse
    {
    }

    [DataContract]
    public class ConvertToModernRequest
    {
        [DataMember]
        public string OldUsername;

        [DataMember]
        public byte[] OldStylePassword;

        [DataMember]
        public string NewUsername;

        [DataMember]
        public string NewStylePassword;

        [DataMember]
        public bool AddDevice;
    }

    [DataContract]
    public class ConvertToModernResponse : PlainResponse
    {
        [DataMember]
        public long AccountId;

        [DataMember]
        public int DeviceId;
    }

    [DataContract]
    public class LoginCredentials
    {
        /// <summary>
        /// If possible, provide the account ID. Otherwise, leave it at default value of 0.
        /// </summary>
        [DataMember]
        public long AccountId;

        /// <summary>
        /// You must always provide the username. Max length is 50, there is no min.
        /// </summary>
        [DataMember]
        public string Username;

        /// <summary>
        /// You must always provide the token.
        /// </summary>
        [DataMember]
        public string Token;

        /// <summary>
        /// Obsolete, use Token instead. Old description: You must always provide the password.
        /// </summary>
        [DataMember]
        [Obsolete("Use Token instead")]
        public string Password;
    }

    [DataContract]
    public class AllDevicesRequest : PartialLoginRequest
    {
    }

    [DataContract]
    public class AllDevicesResponse : PlainResponse
    {
        [DataMember]
        public List<DeviceInfo> Devices = new List<DeviceInfo>();
    }

    [DataContract]
    public class StatusMessageResponse : PlainResponse
    {
        /// <summary>
        /// Guaranteed to not be null by the server.
        /// </summary>
        [DataMember]
        public string Message = "";
    }

    [DataContract]
    public class DeviceInfo
    {
        [DataMember]
        public int DeviceId { get; set; }

        /// <summary>
        /// Returns "Last Connected: [LastConnected.ToString()]"
        /// </summary>
        public string LastConnectedString
        {
            get { return "Last Connected: " + LastConnected.ToLocalTime().ToString(); }
        }

        [DataMember]
        public DateTime LastConnected = DateTime.MinValue;

        [DataMember]
        public string Platform;

        [DataMember]
        public string AppName;

        [DataMember]
        public string AppVersion;
    }

    [DataContract]
    public class UploadImageRequest : PartialLoginRequest
    {
        /// <summary>
        /// Should be a randomly generated GUID
        /// </summary>
        [DataMember]
        public string ImageName;

        [DataMember]
        public byte[] ImageData;

        [DataMember]
        public bool AppendToEnd;
    }

    [DataContract]
    public class UploadImageResponse : PlainResponse
    {
    }

    [DataContract]
    public class UpdatedItems
    {
        public UpdatedItems() { }

        public UpdatedItems(IEnumerable<BaseItem> items)
        {
            AddRange(items);
        }

        [DataMember]
        public List<Year> Years = new List<Year>();

        [DataMember]
        public List<Semester> Semesters = new List<Semester>();

        [DataMember]
        public List<Class> Classes = new List<Class>();

        [DataMember]
        public List<MegaItem> MegaItems = new List<MegaItem>();

        [DataMember]
        public List<Task> Tasks = new List<Task>();


        [DataMember]
        public List<Homework> Homeworks = new List<Homework>();

        [DataMember]
        public List<Exam> Exams = new List<Exam>();

        [DataMember]
        public List<Schedule> Schedules = new List<Schedule>();

        [DataMember]
        public List<WeightCategory> WeightCategories = new List<WeightCategory>();

        [DataMember]
        public List<Grade> Grades = new List<Grade>();

        private class UpdatedEnumerable : IEnumerable<BaseItem>
        {
            private UpdatedItems _updatedItems;

            public UpdatedEnumerable(UpdatedItems updatedItems)
            {
                _updatedItems = updatedItems;
            }

            public IEnumerator<BaseItem> GetEnumerator()
            {
                return new IEnumerableLinker<BaseItem>(

                    // Level 0
                    _updatedItems.Years,

                    // Level 1
                    _updatedItems.Semesters,

                    // Level 2
                    _updatedItems.Classes,
                    _updatedItems.Tasks,

                    // Level 3 (or some MegaItems can be level 2, but that's fine)
                    _updatedItems.MegaItems,
                    _updatedItems.Homeworks,
                    _updatedItems.Exams,
                    _updatedItems.Schedules,
                    _updatedItems.WeightCategories,

                    // Level 4
                    _updatedItems.Grades).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private IEnumerable<BaseItem> _asEnumerable;
        public IEnumerable<BaseItem> AsEnumerable()
        {
            if (_asEnumerable == null)
            {
                _asEnumerable = new UpdatedEnumerable(this);
            }

            return _asEnumerable;
        }

        public void Add(BaseItem item, bool checkIfExists = false)
        {
            if (item is Year)
                AddHelper(Years, item, checkIfExists);

            else if (item is Semester)
                AddHelper(Semesters, item, checkIfExists);

            else if (item is Class)
                AddHelper(Classes, item, checkIfExists);

            else if (item is Task)
                AddHelper(Tasks, item, checkIfExists);

            else if (item is Homework)
                AddHelper(Homeworks, item, checkIfExists);

            else if (item is Exam)
                AddHelper(Exams, item, checkIfExists);

            else if (item is Schedule)
                AddHelper(Schedules, item, checkIfExists);

            else if (item is WeightCategory)
                AddHelper(WeightCategories, item, checkIfExists);

            else if (item is Grade)
                AddHelper(Grades, item, checkIfExists);

            else if (item is MegaItem)
                AddHelper(MegaItems, item, checkIfExists);

            else
                throw new NotImplementedException("Item wasn't any of the supported types.");
        }

        private void AddHelper<T>(List<T> items, BaseItem itemToAdd, bool checkIfExists)
            where T : BaseItem
        {
            if (checkIfExists)
            {
                if (items.Any(i => i.Identifier == itemToAdd.Identifier))
                {
                    return;
                }
            }

            items.Add(itemToAdd as T);
        }

        public void AddRange(IEnumerable<BaseItem> items)
        {
            IEnumerator<BaseItem> i = items.GetEnumerator();
            while (i.MoveNext())
                Add(i.Current);
        }

        public void AddRangeAvoidDuplicates<T>(IEnumerable<T> adding)
            where T : BaseItem
        {
            foreach (var item in adding)
            {
                Add(item, checkIfExists: true);
            }
        }

        public void MergeEverythingExceptMegaItemsAndGrades(UpdatedItems other)
        {
            Years.AddRange(other.Years);
            Semesters.AddRange(other.Semesters);
            Classes.AddRange(other.Classes);
            Schedules.AddRange(other.Schedules);
            WeightCategories.AddRange(other.WeightCategories);
        }

        public void MergeMegaItemsAvoidDuplicates(UpdatedItems other)
        {
            AddRangeAvoidDuplicates(other.MegaItems);
        }
    }

    [DataContract]
    public class DeletedItem
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public DateTime DeletedOn;

        public DeletedItem(Guid identifier, DateTime deletedOn)
        {
            Identifier = identifier;
            DeletedOn = deletedOn;
        }
    }

    [DataContract]
    public class WebsiteRequest : PartialLoginRequest
    {
        [DataMember]
        public UpdatedItems UpdatedItems;

        [DataMember]
        public IEnumerable<DeletedItem> DeletedItems;

        private Dictionary<Guid, DateTime> _deleteRequests;
        /// <summary>
        /// This is not stored and can be ignored from the API.
        /// </summary>
        public Dictionary<Guid, DateTime> DeleteRequests
        {
            get
            {
                if (_deleteRequests == null)
                {
                    if (DeletedItems != null)
                        _deleteRequests = DeletedItems.ToDictionary(i => i.Identifier, i => i.DeletedOn);
                    else
                        _deleteRequests = new Dictionary<Guid, DateTime>();
                }

                return _deleteRequests;
            }
        }
    }

    [DataContract]
    public class QuickClass
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public string Name;

        [DataMember]
        public string Color;
    }

    [DataContract]
    public class QuickWeightCategory
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public string Name;

        [DataMember]
        public double WeightValue;
    }

    [DataContract]
    public class QuickGrade
    {
        [DataMember]
        public Guid Identifier;

        [DataMember]
        public string Name;

        [DataMember]
        public double GradeReceived;

        [DataMember]
        public double GradeTotal;

        [DataMember]
        public bool IsDropped;

        [DataMember]
        public double IndividualWeight;
    }

    /// <summary>
    /// This request pulls down all the classes in a semester, and also pulls down the homework, exams, and schedules for each of those classes.
    /// </summary>
    [DataContract]
    public class GetSemesterContentRequest : WebsiteRequest
    {
        /// <summary>
        /// The semester you want to download info from
        /// </summary>
        [DataMember]
        public Guid SemesterIdentifier;
    }

    [DataContract]
    public class GetSemesterContentResponse : PlainResponse
    {
        /// <summary>
        /// An array of all the classes inside the requested semester. Will always be initialized unless Error is set.
        /// </summary>
        [DataMember]
        public List<ClassWithChildren> Classes;
    }

    [DataContract]
    public class ExportSemesterRequest : PartialLoginRequest
    {
        /// <summary>
        /// The semester you want to export
        /// </summary>
        [DataMember]
        public Guid SemesterIdentifier;
    }

    [DataContract]
    public class ExportSemesterResponse : PlainResponse
    {
        [DataMember]
        public string UrlForSharing;
    }

    [DataContract]
    public class ImportRequest : PartialLoginRequest
    {
        /// <summary>
        /// The identifier that was shared
        /// </summary>
        [DataMember]
        public Guid ImportIdentifier;

        /// <summary>
        /// The local current time, used to determine which tasks should be marked "completed"
        /// </summary>
        [DataMember]
        public DateTime CurrentTime = DateTime.UtcNow;
    }

    [DataContract]
    public class ImportResponse : PlainResponse
    {
        /// <summary>
        /// The semester identifier that was imported
        /// </summary>
        [DataMember]
        public Guid? ImportedSemesterIdentifier;
    }

    [DataContract]
    public class SyncRequest : PartialLoginRequest
    {
        [DataMember]
        public long DeviceId;

        [DataMember]
        public IEnumerable<Dictionary<string, object>> Updates;

        [DataMember]
        public IEnumerable<Guid> Deletes;

        [DataMember]
        public IEnumerable<ItemType> ReSyncNeededFor;

        [DataMember]
        public IEnumerable<MegaItemType> MegaItemReSyncNeededFor;

        /// <summary>
        /// This is the old way
        /// </summary>
        //[DataMember]
        //public UpdatedItems UpdatedItems;

        /// <summary>
        /// This is the old way
        /// </summary>
        //[DataMember]
        //public IEnumerable<DeletedItem> DeletedItems;

        //private Dictionary<Guid, DateTime> _deleteRequests;
        ///// <summary>
        ///// This is not stored and can be ignored from the API.
        ///// </summary>
        //public Dictionary<Guid, DateTime> DeleteRequests
        //{
        //    get
        //    {
        //        if (_deleteRequests == null)
        //        {
        //            if (DeletedItems != null)
        //                _deleteRequests = DeletedItems.ToDictionary(i => i.Identifier, i => i.DeletedOn);
        //            else
        //                _deleteRequests = new Dictionary<Guid, DateTime>();
        //        }

        //        return _deleteRequests;
        //    }
        //}

        /// <summary>
        /// Your device must store this number. Every time you sync you'll get a ChangeNumber to represent what version of the data your device currently received. Should be 0 the first time you initiate a sync.
        /// </summary>
        [DataMember]
        public int CurrentChangeNumber;

        [DataMember]
        public string PushChannel = "";

        [DataMember]
        public string Platform;

        [DataMember]
        public string AppName;

        [DataMember]
        public string AppVersion;

        [DataMember]
        public int SyncVersion;

        [DataMember]
        public int MaxItemsToReturn = int.MaxValue;

        /// <summary>
        /// Specify this to load the next page of results
        /// </summary>
        [DataMember]
        public string Page;

        /// <summary>
        /// Client specifies this, and if online account is different, server returns the new grade scale
        /// </summary>
        [DataMember]
        public long? CurrentDefaultGradeScaleIndex;

        /// <summary>
        /// Adds the offset to updated and deleted items.
        /// </summary>
        /// <param name="offset">Offset from actual time, in milliseconds.</param>
        public void ApplyOffset(int offset)
        {
            if (offset == 0)
                return;

            //if (UpdatedItems != null)
            //{
            //    foreach (BaseItem item in UpdatedItems.AsEnumerable())
            //        item.ApplyOffset(offset);
            //}

            //if (DeletedItems != null)
            //{
            //    foreach (DeletedItem item in DeletedItems)
            //        item.DeletedOn.AddMilliseconds(offset);
            //}
        }
    }

    [DataContract]
    public class SyncSettingsRequest : PartialLoginRequest
    {
        [DataMember]
        public SyncedSettings Settings;
    }

    [DataContract]
    public class SyncSettingsResponse : PlainResponse
    {
        /// <summary>
        /// Device should save this number and then send it up with next normal syncs.
        /// </summary>
        [DataMember]
        public long DefaultGradeScaleIndex;
    }

    /// <summary>
    /// Supports partial syncs by making all values nullable, so that you only need to provide values that changed.
    /// </summary>
    [DataContract]
    public class SyncedSettings
    {
        /// <summary>
        /// Some countries calculate final grades differently. Mexico, for example, simply calculates using percents instead of GPA's. This option lets users keep their settings synced across devices.
        /// </summary>
        [DataMember]
        public GpaOptions? GpaOption;

        /// <summary>
        /// This option is used to figure out what week it currently is. Only the Date component matters. On this date, it will be week one. Exactly 7 days after this date it should be week 2. And after 14 days, it should be week 1 again. Be aware that this value could be greater than the current day and still be valid (if today was 1/5/2014, and this value was 1/7/2014, then the app should realize it currently is week 2). Date should be stored as UTC so that no time conversions occur. Must, however, be a valid SQL date.
        /// </summary>
        [DataMember]
        public DateTime? WeekOneStartsOn;

        /// <summary>
        /// Added in 5.3.14.0. Sync which semester is selected. Note that if this is null, no changes will be made (null also allows down-level clients to not accidently clear out values).
        /// </summary>
        [DataMember]
        public Guid? SelectedSemesterId;

        /// <summary>
        /// Added on 3/12/2020, IANA time format. If null then ignored/not set.
        /// </summary>
        [DataMember]
        public string SchoolTimeZone;

        /// <summary>
        /// Added on 4/28/2021. If null, then ignored/not set.
        /// </summary>
        [DataMember]
        public GradeScale[] DefaultGradeScale;

        /// <summary>
        /// Added on 4/28/2021. If null, then ignored/not set.
        /// </summary>
        [DataMember]
        public bool? DefaultDoesRoundGradesUp;

        /// <summary>
        /// Added on 4/28/2021. If null, then ignored/not set.
        /// </summary>
        [DataMember]
        public bool? DefaultDoesAverageGradeTotals;
    }

    [DataContract]
    public class SyncResponse : PlainResponse
    {
        public static readonly string NO_ACCOUNT = "No account under that username exists. Check your username.";
        public static readonly string USERNAME_CHANGED = "Your username doesn't match your account. Maybe you've changed your username?";
        public static readonly string INCORRECT_PASSWORD = "Incorrect password.";
        public static readonly string DEVICE_NOT_FOUND = "Couldn't find your device. Maybe it was deleted?";
        public static readonly string NO_DEVICE = "No device is set.";
        public static readonly string NO_PASSWORD = "No password was inputted.";
        public static readonly string NO_USERNAME = "No username was inputted.";
        public static readonly string INCORRECT_CREDENTIALS = "Incorrect credentials.";
        public static readonly string USERNAME_ALREADY_EXISTS = "That username already exists. Try a different username.";

        /// <summary>
        /// You must store this number on the device and send it up with each sync. This allows the sync system to only send down items newer than whatever ChangeNumber you currently are at.
        /// </summary>
        [DataMember]
        public int ChangeNumber;

        /// <summary>
        /// Server only returns this if default grade scale has changed.
        /// </summary>
        [DataMember]
        public long? DefaultGradeScaleIndex;

        /// <summary>
        /// A list of the updated (or new) items since the last successful sync.
        /// </summary>
        [DataMember]
        public UpdatedItems UpdatedItems;

        [DataMember]
        public IEnumerable<DeletedItem> DeletedItems;

        [DataMember]
        public SyncedSettings Settings;

        [DataMember]
        public DateTime PremiumAccountExpiresOn;

        public void ApplyOffset(int offset)
        {
            if (offset == 0)
                return;

            offset = offset * -1;

            if (UpdatedItems != null)
                foreach (BaseItem item in UpdatedItems.AsEnumerable())
                    item.ApplyOffset(offset);

            if (DeletedItems != null)
                foreach (DeletedItem item in DeletedItems)
                    item.DeletedOn.AddMilliseconds(offset);
        }

        [DataMember]
        public List<UpdateError> UpdateErrors = new List<UpdateError>();

        [DataContract]
        public class UpdateError
        {
            public enum ErrorType
            {
                IdentifierEmpty,

                Updated_BelowMinSqlDate,
                Updated_AboveMaxSqlDate,

                DateCreated_BelowMinSqlDate,
                DateCreated_AboveMaxSqlDate,

                SqlError
            }

            [DataMember]
            public Guid Identifier;

            [DataMember]
            public ErrorType Error;

            [DataMember]
            public int SqlErrorNumber;

            [DataMember]
            public string SqlErrorMessage;

            public override string ToString()
            {
                return Error + "\n\tIdentifier: " + Identifier + "\n\tSqlErrorNumber: " + SqlErrorNumber + "\n\tSqlErrorMessage: " + SqlErrorMessage;
            }
        }

        [DataMember]
        public string NextPage;
    }

    [DataContract]
    public class ChangePasswordRequest : PartialLoginRequest
    {
        /// <summary>
        /// OldLogin is kept for legacy purposes. Apps should set OldLogin.
        /// </summary>
        [DataMember]
        public LoginCredentials OldLogin;


        public override LoginCredentials Login
        {
            get
            {
                return OldLogin;
            }
            set
            {
                //nothing
            }
        }

        [DataMember]
        public string NewPassword;
    }

    [DataContract]
    public class ChangePasswordResponse : PlainResponse
    {
        [DataMember]
        public string Session { get; set; }
    }

    /// <summary>
    /// This is rarely used. The usage situation is as follows:
    /// <br></br><br></br>
    ///   1) User has previously logged in on the device before.
    ///   2) User currently logged out of their account on the device.
    ///   3) While logged out, the user changed their online password from another device.
    ///   4) Now user tries to log in using their new password.
    ///   5) Device notices that the username matches one of the accounts, but the password isn't equal.
    ///   6) Device then takes the AccountId, Username, and the password the user entered, and sends a CheckPasswordResponse.
    ///   7) If the response comes back without an error, then the new password is correct. The device updates the locally-stored password, and logs the user in.
    ///   8) However, if the response had an error, the device would tell the user that their password didn't match.
    /// </summary>
    [DataContract]
    public class CheckPasswordRequest
    {
        /// <summary>
        /// If possible, provide the account ID. If you don't have it, it'll locate their account by their username.
        /// </summary>
        [DataMember]
        public long AccountId;

        /// <summary>
        /// You must provide their username.
        /// </summary>
        [DataMember]
        public string Username;

        /// <summary>
        /// This is the new password the user entered, that doesn't match your local storage.
        /// </summary>
        [DataMember]
        public string PasswordToTry;
    }

    [DataContract]
    public class CheckPasswordResponse : PlainResponse
    {
        [DataMember]
        public string Session;
    }

    [DataContract]
    public class ChangeUsernameRequest : PartialLoginRequest
    {
        [DataMember]
        public string NewUsername;
    }

    [DataContract]
    public class ChangeUsernameResponse : PlainResponse
    {
    }

    [DataContract]
    public class UndeleteItemRequest : PartialLoginRequest
    {
        /// <summary>
        /// The identifier of the item to undelete
        /// </summary>
        [DataMember]
        public Guid Identifier;
    }

    [DataContract]
    public class UpgradeAccountInfo
    {
        [DataMember]
        public string NewUsername;

        [DataMember]
        public string NewPassword;

        [DataMember]
        public string OldUsername;

        [DataMember]
        public byte[] OldPassword;
    }

    [DataContract]
    public class LoginModernRequest
    {
        [DataMember]
        public string Username;

        [DataMember]
        public string Password;
    }

    [DataContract]
    public class LoginModernResponse : PlainResponse
    {
        [DataMember]
        public long AccountId;

        /// <summary>
        /// The hashed password, web version uses this
        /// </summary>
        [DataMember]
        public string Session;
    }

    [DataContract]
    public class AddPremiumAccountDurationRequest : PartialLoginRequest
    {
        [DataMember]
        public int DaysToAdd;
    }

    [DataContract]
    public class AddPremiumAccountDurationResponse : PlainResponse
    {
        //nothing
    }

    [DataContract]
    public class ViewAllPicturesRequest : PartialLoginRequest
    {
        /// <summary>
        /// The index of photos to start fetching at
        /// </summary>
        [DataMember]
        public int Start = 0;

        /// <summary>
        /// The upper limit on how many photos should be returned. Default is 0, which fetches all.
        /// </summary>
        [DataMember]
        public int Count = 0;
    }

    [DataContract]
    public class ViewAllPicturesResponse : PlainResponse
    {
        [DataMember]
        public int Start;

        /// <summary>
        /// Array of urls of the images.
        /// </summary>
        [DataMember]
        public string[] Pictures;

        /// <summary>
        /// The number of remaining pictures after these that weren't fetched.
        /// </summary>
        [DataMember]
        public int Remaining;
    }

    [DataContract]
    public class ShouldSuggestOtherPlatformsRequest : PartialLoginRequest
    {
        [DataMember]
        public string CurrentPlatform { get; set; }
    }

    [DataContract]
    public class ShouldSuggestOtherPlatformsResponse : PlainResponse
    {
        [DataMember]
        public bool ShouldSuggest { get; set; }
    }

    public class PastDeletedItem
    {
        public Guid Identifier { get; set; }
        public ItemType ItemType { get; set; }
        public string Name { get; set; }
        public int ChangeNumber { get; set; }
        public DateTime Updated { get; set; }
        public DateTime DateCreated { get; set; }
        public int CountOfChildren { get; set; }
        public string ChildrenPreview { get; set; }
    }

    [DataContract]
    public class GetDeletedYearsAndSemestersResponse : PlainResponse
    {
        [DataMember]
        public PastDeletedItem[] DeletedItems { get; set; }
    }

    [DataContract]
    public class GetDeletedClassesResponse : PlainResponse
    {
        [DataMember]
        public PastDeletedItem[] DeletedItems { get; set; }
    }
}
