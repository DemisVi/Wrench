namespace ROOT.CIMV2.Win32 {
    using System;
    using System.ComponentModel;
    using System.Management;
    using System.Collections;
    using System.Globalization;
    
    
    // Функции ShouldSerialize<PropertyName> используются обозревателем свойств VS чтобы проверить, должно ли конкретное свойство быть сериализовано. Эти функции добавляются для всех свойств ValueType (свойства типа Int32, BOOL и т.д., для которых не может быть установлено значение null). Эти функции используют функцию Is<PropertyName>Null. Также эти функции используются в реализации TypeConverter для свойств для проверки значения NULL свойства, чтобы пустое значение могло быть отображено в обозревателе свойств для перетаскивания и копирования в Visual studio.
    // Функции Is<PropertyName>Null() используются, чтобы проверить, имеет ли свойство значение NULL.
    // Функции Reset<PropertyName> добавлены для свойств чтения/записи Nullable. Эти функции используются VS designer в обозревателе свойств, чтобы установить для свойства значение NULL.
    // Каждое свойство, добавленное к классу для свойства WMI, имеет набор атрибутов, который определяет его поведение в Visual Studio designer, а также определяет используемый TypeConverter.
    // Функции преобразования даты и времени ToDateTime и ToDmtfDateTime добавлены к классу для преобразования даты и времени DMTF в System.DateTime и наоборот.
    // Класс Early Bound создан для класса WMI.Win32_POTSModem
    public class POTSModem : System.ComponentModel.Component {
        
        // Частное свойство, которое хранит пространство имен WMI, в котором находится этот класс.
        private static string CreatedWmiNamespace = "root\\CimV2";
        
        // Частное свойство, которое хранит имя класса WMI, создавшее этот класс.
        private static string CreatedClassName = "Win32_POTSModem";
        
        // Частное поле для хранения ManagementScope, используемое различными методами.
        private static System.Management.ManagementScope statMgmtScope = null;
        
        private ManagementSystemProperties PrivateSystemProperties;
        
        // Зависимый объект WMI lateBound.
        private System.Management.ManagementObject PrivateLateBoundObject;
        
        // Поле для хранения поведения "автоматической отправки" класса.
        private bool AutoCommitProp;
        
        // Частная переменная, которая хранит встроенное свойство, соответствующее экземпляру.
        private System.Management.ManagementBaseObject embeddedObj;
        
        // Текущий используемый объект WMI
        private System.Management.ManagementBaseObject curObj;
        
        // Флаг, указывающий, является ли экземпляр встроенным объектом.
        private bool isEmbedded;
        
        // Ниже приводятся различные перезагрузки конструкторов для инициализации экземпляра класса с объектом WMI.
        public POTSModem() {
            this.InitializeObject(null, null, null);
        }
        
        public POTSModem(string keyDeviceID) {
            this.InitializeObject(null, new System.Management.ManagementPath(POTSModem.ConstructPath(keyDeviceID)), null);
        }
        
        public POTSModem(System.Management.ManagementScope mgmtScope, string keyDeviceID) {
            this.InitializeObject(((System.Management.ManagementScope)(mgmtScope)), new System.Management.ManagementPath(POTSModem.ConstructPath(keyDeviceID)), null);
        }
        
        public POTSModem(System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            this.InitializeObject(null, path, getOptions);
        }
        
        public POTSModem(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path) {
            this.InitializeObject(mgmtScope, path, null);
        }
        
        public POTSModem(System.Management.ManagementPath path) {
            this.InitializeObject(null, path, null);
        }
        
        public POTSModem(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            this.InitializeObject(mgmtScope, path, getOptions);
        }
        
        public POTSModem(System.Management.ManagementObject theObject) {
            Initialize();
            if ((CheckIfProperClass(theObject) == true)) {
                PrivateLateBoundObject = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
                curObj = PrivateLateBoundObject;
            }
            else {
                throw new System.ArgumentException("Имя класса не совпадает.");
            }
        }
        
        public POTSModem(System.Management.ManagementBaseObject theObject) {
            Initialize();
            if ((CheckIfProperClass(theObject) == true)) {
                embeddedObj = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(theObject);
                curObj = embeddedObj;
                isEmbedded = true;
            }
            else {
                throw new System.ArgumentException("Имя класса не совпадает.");
            }
        }
        
        // Свойство возвращает пространство имен класса WMI.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string OriginatingNamespace {
            get {
                return "root\\CimV2";
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ManagementClassName {
            get {
                string strRet = CreatedClassName;
                if ((curObj != null)) {
                    if ((curObj.ClassPath != null)) {
                        strRet = ((string)(curObj["__CLASS"]));
                        if (((strRet == null) 
                                    || (strRet == string.Empty))) {
                            strRet = CreatedClassName;
                        }
                    }
                }
                return strRet;
            }
        }
        
        // Свойство, указывающее встроенный объект для свойств get System объекта WMI.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementSystemProperties SystemProperties {
            get {
                return PrivateSystemProperties;
            }
        }
        
        // Свойство, возвращающее объект lateBound.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementBaseObject LateBoundObject {
            get {
                return curObj;
            }
        }
        
        // ManagementScope объекта.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementScope Scope {
            get {
                if ((isEmbedded == false)) {
                    return PrivateLateBoundObject.Scope;
                }
                else {
                    return null;
                }
            }
            set {
                if ((isEmbedded == false)) {
                    PrivateLateBoundObject.Scope = value;
                }
            }
        }
        
        // Свойство, которое отображает поведение выделения для объекта WMI. Если оно имеет значение "истина", объект WMI будет автоматически сохраняться после любого изменения свойства. (Т.е. после изменения свойства вызывается Put()).
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCommit {
            get {
                return AutoCommitProp;
            }
            set {
                AutoCommitProp = value;
            }
        }
        
        // ManagementPath зависимого объекта WMI.
        [Browsable(true)]
        public System.Management.ManagementPath Path {
            get {
                if ((isEmbedded == false)) {
                    return PrivateLateBoundObject.Path;
                }
                else {
                    return null;
                }
            }
            set {
                if ((isEmbedded == false)) {
                    if ((CheckIfProperClass(null, value, null) != true)) {
                        throw new System.ArgumentException("Имя класса не совпадает.");
                    }
                    PrivateLateBoundObject.Path = value;
                }
            }
        }
        
        // Свойство общей статической области, используемое различными методами.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static System.Management.ManagementScope StaticScope {
            get {
                return statMgmtScope;
            }
            set {
                statMgmtScope = value;
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAnswerModeNull {
            get {
                if ((curObj["AnswerMode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Answer mode defines the current auto-answer/call-back setting for the modem.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public AnswerModeValues AnswerMode {
            get {
                if ((curObj["AnswerMode"] == null)) {
                    return ((AnswerModeValues)(System.Convert.ToInt32(6)));
                }
                return ((AnswerModeValues)(System.Convert.ToInt32(curObj["AnswerMode"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The AttachedTo property indicates the port to which the POTS modem is attached.\nE" +
            "xample: COM1.")]
        public string AttachedTo {
            get {
                return ((string)(curObj["AttachedTo"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAvailabilityNull {
            get {
                if ((curObj["Availability"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The availability and status of the device.  For example, the Availability property indicates that the device is running and has full power (value=3), or is in a warning (4), test (5), degraded (10) or power save state (values 13-15 and 17). Regarding the power saving states, these are defined as follows: Value 13 (""Power Save - Unknown"") indicates that the device is known to be in a power save mode, but its exact status in this mode is unknown; 14 (""Power Save - Low Power Mode"") indicates that the device is in a power save state but still functioning, and may exhibit degraded performance; 15 (""Power Save - Standby"") describes that the device is not functioning but could be brought to full power 'quickly'; and value 17 (""Power Save - Warning"") indicates that the device is in a warning state, though also in a power save mode.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public AvailabilityValues Availability {
            get {
                if ((curObj["Availability"] == null)) {
                    return ((AvailabilityValues)(System.Convert.ToInt32(0)));
                }
                return ((AvailabilityValues)(System.Convert.ToInt32(curObj["Availability"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The BlindOff property contains the command string used to detect a dial tone befo" +
            "re dialing.\nExample: X4.")]
        public string BlindOff {
            get {
                return ((string)(curObj["BlindOff"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The BlindOn property contains the command string used to dial whether or not ther" +
            "e is a dial tone.\nExample: X3.")]
        public string BlindOn {
            get {
                return ((string)(curObj["BlindOn"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Caption {
            get {
                return ((string)(curObj["Caption"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The CompatibilityFlags property lists all modem connection protocols with which t" +
            "his modem device is compatible.")]
        public string CompatibilityFlags {
            get {
                return ((string)(curObj["CompatibilityFlags"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCompressionInfoNull {
            get {
                if ((curObj["CompressionInfo"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Describes the data compression characteristics of the modem.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public CompressionInfoValues CompressionInfo {
            get {
                if ((curObj["CompressionInfo"] == null)) {
                    return ((CompressionInfoValues)(System.Convert.ToInt32(5)));
                }
                return ((CompressionInfoValues)(System.Convert.ToInt32(curObj["CompressionInfo"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The CompressionOff property contains the command string used to disable hardware " +
            "data compression.\nExample: S46=136.")]
        public string CompressionOff {
            get {
                return ((string)(curObj["CompressionOff"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The CompressionOn property contains the command string used to enable hardware da" +
            "ta compression.\nExample: S46=138.")]
        public string CompressionOn {
            get {
                return ((string)(curObj["CompressionOn"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsConfigManagerErrorCodeNull {
            get {
                if ((curObj["ConfigManagerErrorCode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Indicates the Win32 Configuration Manager error code.  The following values may b" +
            "e returned: \n0      This device is working properly. \n1      This device is not " +
            "configured correctly. \n2      Windows cannot load the driver for this device. \n3" +
            "      The driver for this device might be corrupted, or your system may be runni" +
            "ng low on memory or other resources. \n4      This device is not working properly" +
            ". One of its drivers or your registry might be corrupted. \n5      The driver for" +
            " this device needs a resource that Windows cannot manage. \n6      The boot confi" +
            "guration for this device conflicts with other devices. \n7      Cannot filter. \n8" +
            "      The driver loader for the device is missing. \n9      This device is not wo" +
            "rking properly because the controlling firmware is reporting the resources for t" +
            "he device incorrectly. \n10     This device cannot start. \n11     This device fai" +
            "led. \n12     This device cannot find enough free resources that it can use. \n13 " +
            "    Windows cannot verify this device\'s resources. \n14     This device cannot wo" +
            "rk properly until you restart your computer. \n15     This device is not working " +
            "properly because there is probably a re-enumeration problem. \n16     Windows can" +
            "not identify all the resources this device uses. \n17     This device is asking f" +
            "or an unknown resource type. \n18     Reinstall the drivers for this device. \n19 " +
            "    Your registry might be corrupted. \n20     Failure using the VxD loader. \n21 " +
            "    System failure: Try changing the driver for this device. If that does not wo" +
            "rk, see your hardware documentation. Windows is removing this device. \n22     Th" +
            "is device is disabled. \n23     System failure: Try changing the driver for this " +
            "device. If that doesn\'t work, see your hardware documentation. \n24     This devi" +
            "ce is not present, is not working properly, or does not have all its drivers ins" +
            "talled. \n25     Windows is still setting up this device. \n26     Windows is stil" +
            "l setting up this device. \n27     This device does not have valid log configurat" +
            "ion. \n28     The drivers for this device are not installed. \n29     This device " +
            "is disabled because the firmware of the device did not give it the required reso" +
            "urces. \n30     This device is using an Interrupt Request (IRQ) resource that ano" +
            "ther device is using. \n31     This device is not working properly because Window" +
            "s cannot load the drivers required for this device.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ConfigManagerErrorCodeValues ConfigManagerErrorCode {
            get {
                if ((curObj["ConfigManagerErrorCode"] == null)) {
                    return ((ConfigManagerErrorCodeValues)(System.Convert.ToInt32(32)));
                }
                return ((ConfigManagerErrorCodeValues)(System.Convert.ToInt32(curObj["ConfigManagerErrorCode"])));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsConfigManagerUserConfigNull {
            get {
                if ((curObj["ConfigManagerUserConfig"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Indicates whether the device is using a user-defined configuration.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool ConfigManagerUserConfig {
            get {
                if ((curObj["ConfigManagerUserConfig"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["ConfigManagerUserConfig"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ConfigurationDialog property contains the modem initialization string. This p" +
            "roperty is comprised of command strings from other members of this class.")]
        public string ConfigurationDialog {
            get {
                return ((string)(curObj["ConfigurationDialog"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CountriesSupported is an array (bag) that contains a set of strings defining the " +
            "countries/regions in which the modem can operate.")]
        public string[] CountriesSupported {
            get {
                return ((string[])(curObj["CountriesSupported"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CountrySelected is a string value defining the country/region for which the modem" +
            " is currently programmed. When multiple countries/regions are supported, this pr" +
            "operty defines which one is currently selected for use.")]
        public string CountrySelected {
            get {
                return ((string)(curObj["CountrySelected"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CreationClassName indicates the name of the class or the subclass used in the cre" +
            "ation of an instance. When used with the other key properties of this class, thi" +
            "s property allows all instances of this class and its subclasses to be uniquely " +
            "identified.")]
        public string CreationClassName {
            get {
                return ((string)(curObj["CreationClassName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CurrentPasswords is an array (bag) containing the currently defined passwords for" +
            " the modem. This array may be left blank for security reasons.")]
        public string[] CurrentPasswords {
            get {
                return ((string[])(curObj["CurrentPasswords"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Device Control Block (DCB) property contains the control settings for a seria" +
            "l communications device, in this case, the modem device.")]
        public byte[] DCB {
            get {
                return ((byte[])(curObj["DCB"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Default property indicates whether this POTS modem is the default modem on th" +
            "e Win32 computer system.")]
        public byte[] Default {
            get {
                return ((byte[])(curObj["Default"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description {
            get {
                return ((string)(curObj["Description"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DeviceID property contains a string uniquely identifying this POTS modem from" +
            " other devices on the system.")]
        public string DeviceID {
            get {
                return ((string)(curObj["DeviceID"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DeviceLoader property indicates the name of the device loader for the modem. " +
            "A device loader loads and manages device drivers and enumerators for a given dev" +
            "ice.")]
        public string DeviceLoader {
            get {
                return ((string)(curObj["DeviceLoader"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DeviceType property indicates the physical type of the modem.")]
        public string DeviceType {
            get {
                return ((string)(curObj["DeviceType"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDialTypeNull {
            get {
                if ((curObj["DialType"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("DialType is an integer indicating whether tone (value=1) or pulse dialing (value=" +
            "2) is used. \"Unknown\" (0) can also be defined.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public DialTypeValues DialType {
            get {
                if ((curObj["DialType"] == null)) {
                    return ((DialTypeValues)(System.Convert.ToInt32(3)));
                }
                return ((DialTypeValues)(System.Convert.ToInt32(curObj["DialType"])));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDriverDateNull {
            get {
                if ((curObj["DriverDate"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DriverDate property indicates the date of the modem driver.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.DateTime DriverDate {
            get {
                if ((curObj["DriverDate"] != null)) {
                    return ToDateTime(((string)(curObj["DriverDate"])));
                }
                else {
                    return System.DateTime.MinValue;
                }
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsErrorClearedNull {
            get {
                if ((curObj["ErrorCleared"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ErrorCleared is a boolean property indicating that the error reported in LastErro" +
            "rCode property is now cleared.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool ErrorCleared {
            get {
                if ((curObj["ErrorCleared"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["ErrorCleared"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ErrorControlForced property contains the command string used to enable error " +
            "correction control when establishing a connection. This increases the reliabilit" +
            "y of the connection.\nExample: +Q5S36=4S48=7.")]
        public string ErrorControlForced {
            get {
                return ((string)(curObj["ErrorControlForced"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsErrorControlInfoNull {
            get {
                if ((curObj["ErrorControlInfo"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Describes the error correction characteristics of the modem.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ErrorControlInfoValues ErrorControlInfo {
            get {
                if ((curObj["ErrorControlInfo"] == null)) {
                    return ((ErrorControlInfoValues)(System.Convert.ToInt32(5)));
                }
                return ((ErrorControlInfoValues)(System.Convert.ToInt32(curObj["ErrorControlInfo"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ErrorControlOff property contains the command string used to disable error co" +
            "ntrol.\nExample: +Q6S36=3S48=128.")]
        public string ErrorControlOff {
            get {
                return ((string)(curObj["ErrorControlOff"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ErrorControlOn property contains the command string used to enable error cont" +
            "rol.\nExample: +Q5S36=7S48=7.")]
        public string ErrorControlOn {
            get {
                return ((string)(curObj["ErrorControlOn"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ErrorDescription is a free-form string supplying more information about the error" +
            " recorded in LastErrorCode property, and information on any corrective actions t" +
            "hat may be taken.")]
        public string ErrorDescription {
            get {
                return ((string)(curObj["ErrorDescription"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The FlowControlHard property contains the command string used to enable hardware " +
            "flow control. Flow control consists of signals sent between computers that verif" +
            "y that both computers are ready to transmit or receive data.\nExample: &K1.")]
        public string FlowControlHard {
            get {
                return ((string)(curObj["FlowControlHard"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The FlowControlOff property contains the command string used to disable flow cont" +
            "rol. Flow control consists of signals sent between computers that verify that bo" +
            "th computers are ready to transmit or receive data.\nExample: &K0.")]
        public string FlowControlOff {
            get {
                return ((string)(curObj["FlowControlOff"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The FlowControlSoft property contains the command string used to enable software " +
            "flow control. Flow control consists of signals sent between computers that verif" +
            "y that both computers are ready to transmit or receive data.\nExample: &K2.")]
        public string FlowControlSoft {
            get {
                return ((string)(curObj["FlowControlSoft"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The InactivityScale property is a multiplier used with the InactivityTimeout prop" +
            "erty to calculate the timeout period of a connection.")]
        public string InactivityScale {
            get {
                return ((string)(curObj["InactivityScale"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInactivityTimeoutNull {
            get {
                if ((curObj["InactivityTimeout"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("InactivityTimeout defines the time limit (in seconds) for automatic disconnection" +
            " of the phone line, if no data is exchanged.  A value of 0 indicates that this f" +
            "eature is present but not enabled.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint InactivityTimeout {
            get {
                if ((curObj["InactivityTimeout"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["InactivityTimeout"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIndexNull {
            get {
                if ((curObj["Index"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Index property indicates the index number for this POTS modem.\nExample: 0")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint Index {
            get {
                if ((curObj["Index"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["Index"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The IndexEx property indicates the device instance ID for this POTS modem.\nExampl" +
            "e: \"1&08\"")]
        public string IndexEx {
            get {
                return ((string)(curObj["IndexEx"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInstallDateNull {
            get {
                if ((curObj["InstallDate"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.DateTime InstallDate {
            get {
                if ((curObj["InstallDate"] != null)) {
                    return ToDateTime(((string)(curObj["InstallDate"])));
                }
                else {
                    return System.DateTime.MinValue;
                }
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsLastErrorCodeNull {
            get {
                if ((curObj["LastErrorCode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("LastErrorCode captures the last error code reported by the logical device.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint LastErrorCode {
            get {
                if ((curObj["LastErrorCode"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["LastErrorCode"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMaxBaudRateToPhoneNull {
            get {
                if ((curObj["MaxBaudRateToPhone"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Defines the maximum settable communication speed for accessing the phone system.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint MaxBaudRateToPhone {
            get {
                if ((curObj["MaxBaudRateToPhone"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["MaxBaudRateToPhone"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMaxBaudRateToSerialPortNull {
            get {
                if ((curObj["MaxBaudRateToSerialPort"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Defines the maximum settable communication speed to the COM port for an external " +
            "modem. Enter 0 if not applicable.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint MaxBaudRateToSerialPort {
            get {
                if ((curObj["MaxBaudRateToSerialPort"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["MaxBaudRateToSerialPort"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMaxNumberOfPasswordsNull {
            get {
                if ((curObj["MaxNumberOfPasswords"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("MaxNumberOfPasswords specifies the number of passwords definable in the modem its" +
            "elf. If this feature is not supported, enter 0.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ushort MaxNumberOfPasswords {
            get {
                if ((curObj["MaxNumberOfPasswords"] == null)) {
                    return System.Convert.ToUInt16(0);
                }
                return ((ushort)(curObj["MaxNumberOfPasswords"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Model property indicates the model of this POTS modem.\nExample: Sportster 56K" +
            " External.")]
        public string Model {
            get {
                return ((string)(curObj["Model"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ModemInfPath property indicates the path to this modem\'s .inf file. This file" +
            " contains initialization information for the modem and its driver.\nExample: C:\\W" +
            "INNT\\INF.")]
        public string ModemInfPath {
            get {
                return ((string)(curObj["ModemInfPath"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ModemInfSection property indicates the name of the section in the modem\'s .in" +
            "f file that contains information about the modem.")]
        public string ModemInfSection {
            get {
                return ((string)(curObj["ModemInfSection"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ModulationBell property contains the command string used to instruct the mode" +
            "m to use Bell modulations for 300 and 1200 bps.\nExample: B1.")]
        public string ModulationBell {
            get {
                return ((string)(curObj["ModulationBell"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ModulationCCITT property contains the command string used to instruct the mod" +
            "em to use CCITT modulations for 300 and 1200 bps.\nExample: B0.")]
        public string ModulationCCITT {
            get {
                return ((string)(curObj["ModulationCCITT"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsModulationSchemeNull {
            get {
                if ((curObj["ModulationScheme"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Describes the modulation scheme of the modem.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ModulationSchemeValues ModulationScheme {
            get {
                if ((curObj["ModulationScheme"] == null)) {
                    return ((ModulationSchemeValues)(System.Convert.ToInt32(12)));
                }
                return ((ModulationSchemeValues)(System.Convert.ToInt32(curObj["ModulationScheme"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Name {
            get {
                return ((string)(curObj["Name"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Indicates the Win32 Plug and Play device ID of the logical device.  Example: *PNP" +
            "030b")]
        public string PNPDeviceID {
            get {
                return ((string)(curObj["PNPDeviceID"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PortSubClass property contains the string definingthe port used for this mode" +
            "m.")]
        public string PortSubClass {
            get {
                return ((string)(curObj["PortSubClass"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Indicates the specific power-related capabilities of the logical device. The array values, 0=""Unknown"", 1=""Not Supported"" and 2=""Disabled"" are self-explanatory. The value, 3=""Enabled"" indicates that the power management features are currently enabled but the exact feature set is unknown or the information is unavailable. ""Power Saving Modes Entered Automatically"" (4) describes that a device can change its power state based on usage or other criteria. ""Power State Settable"" (5) indicates that the SetPowerState method is supported. ""Power Cycling Supported"" (6) indicates that the SetPowerState method can be invoked with the PowerState input variable set to 5 (""Power Cycle""). ""Timed Power On Supported"" (7) indicates that the SetPowerState method can be invoked with the PowerState input variable set to 5 (""Power Cycle"") and the Time parameter set to a specific date and time, or interval, for power-on.")]
        public PowerManagementCapabilitiesValues[] PowerManagementCapabilities {
            get {
                System.Array arrEnumVals = ((System.Array)(curObj["PowerManagementCapabilities"]));
                PowerManagementCapabilitiesValues[] enumToRet = new PowerManagementCapabilitiesValues[arrEnumVals.Length];
                int counter = 0;
                for (counter = 0; (counter < arrEnumVals.Length); counter = (counter + 1)) {
                    enumToRet[counter] = ((PowerManagementCapabilitiesValues)(System.Convert.ToInt32(arrEnumVals.GetValue(counter))));
                }
                return enumToRet;
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPowerManagementSupportedNull {
            get {
                if ((curObj["PowerManagementSupported"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Boolean indicating that the Device can be power managed - ie, put into a power save state. This boolean does not indicate that power management features are currently enabled, or if enabled, what features are supported. Refer to the PowerManagementCapabilities array for this information. If this boolean is false, the integer value 1, for the string, ""Not Supported"", should be the only entry in the PowerManagementCapabilities array.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool PowerManagementSupported {
            get {
                if ((curObj["PowerManagementSupported"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["PowerManagementSupported"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Prefix property indicates the dialing prefix used to access an outside line.")]
        public string Prefix {
            get {
                return ((string)(curObj["Prefix"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Properties property is a string array that lists all the properties (and thei" +
            "r values) for this modem.")]
        public byte[] Properties {
            get {
                return ((byte[])(curObj["Properties"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ProviderName property indicates the network path to the computer that provide" +
            "s the modem services.")]
        public string ProviderName {
            get {
                return ((string)(curObj["ProviderName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Pulse property contains the command string used to instruct the modem to use " +
            "pulse mode for dialing. Pulse dialing is necessary for phone lines that are unab" +
            "le to handle tone dialing.\nExample: P.")]
        public string Pulse {
            get {
                return ((string)(curObj["Pulse"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Reset property contains the command string used to reset the modem for the ne" +
            "xt call.\nExample: AT&F.")]
        public string Reset {
            get {
                return ((string)(curObj["Reset"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ResponsesKeyName property contains the response this modem might report to the operating system during the connection process. The first two characters specify the type of response. The second two characters specify information about the connection being made. The second two characters are used only for Negotiation Progress or Connect response codes. The next eight characters specify the modem-to-modem line speed negotiated in bits per second (bps). The characters represent a 32-bit integer doubleword format (byte and word reversed). The last eight characters indicate that the modem is changing to a different port or Data Terminal Equipment (DTE) speed. Usually this field is not used because modems make connections at a ""locked"" port speed regardless of the modem-to-modem or Data Communications Equipment (DCE) speed.")]
        public string ResponsesKeyName {
            get {
                return ((string)(curObj["ResponsesKeyName"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRingsBeforeAnswerNull {
            get {
                if ((curObj["RingsBeforeAnswer"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Number of rings before the modem answers an incoming call.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public byte RingsBeforeAnswer {
            get {
                if ((curObj["RingsBeforeAnswer"] == null)) {
                    return System.Convert.ToByte(0);
                }
                return ((byte)(curObj["RingsBeforeAnswer"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerModeDial property contains the command string used to turn the modem s" +
            "peaker on after dialing a number, and turning the speaker off when a connection " +
            "has been established.\nExample: M1.")]
        public string SpeakerModeDial {
            get {
                return ((string)(curObj["SpeakerModeDial"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerModeOff property contains the command string used to turn the modem sp" +
            "eaker off.\nExample: M0.")]
        public string SpeakerModeOff {
            get {
                return ((string)(curObj["SpeakerModeOff"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerModeOn property contains the command string used to turn the modem spe" +
            "aker on.\nExample: M2.")]
        public string SpeakerModeOn {
            get {
                return ((string)(curObj["SpeakerModeOn"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerModeSetup property contains the command string used to instruct the mo" +
            "dem to turn the speaker on (until a connection is established).\nExample: M3.")]
        public string SpeakerModeSetup {
            get {
                return ((string)(curObj["SpeakerModeSetup"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerVolumeHigh property contains the command string used to set the modem " +
            "speaker to the highest volume.\nExample: L3.")]
        public string SpeakerVolumeHigh {
            get {
                return ((string)(curObj["SpeakerVolumeHigh"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSpeakerVolumeInfoNull {
            get {
                if ((curObj["SpeakerVolumeInfo"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Describes the volume level of the audible tones from the modem. For example, high" +
            ", medium or low volume can be reported (values 3, 4 or 5, respectively).")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public SpeakerVolumeInfoValues SpeakerVolumeInfo {
            get {
                if ((curObj["SpeakerVolumeInfo"] == null)) {
                    return ((SpeakerVolumeInfoValues)(System.Convert.ToInt32(8)));
                }
                return ((SpeakerVolumeInfoValues)(System.Convert.ToInt32(curObj["SpeakerVolumeInfo"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerVolumeLow property contains the command string used to set the modem s" +
            "peaker to the lowest volume.\nExample: L1.")]
        public string SpeakerVolumeLow {
            get {
                return ((string)(curObj["SpeakerVolumeLow"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SpeakerVolumeMed property contains the command string used to set the modem s" +
            "peaker to a medium volume.\nExample: L2.")]
        public string SpeakerVolumeMed {
            get {
                return ((string)(curObj["SpeakerVolumeMed"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Status {
            get {
                return ((string)(curObj["Status"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsStatusInfoNull {
            get {
                if ((curObj["StatusInfo"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("StatusInfo is a string indicating whether the logical device is in an enabled (va" +
            "lue = 3), disabled (value = 4) or some other (1) or unknown (2) state. If this p" +
            "roperty does not apply to the logical device, the value, 5 (\"Not Applicable\"), s" +
            "hould be used.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public StatusInfoValues StatusInfo {
            get {
                if ((curObj["StatusInfo"] == null)) {
                    return ((StatusInfoValues)(System.Convert.ToInt32(0)));
                }
                return ((StatusInfoValues)(System.Convert.ToInt32(curObj["StatusInfo"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The StringFormat property describes the type of characters used for text passed t" +
            "hrough the modem.")]
        public string StringFormat {
            get {
                return ((string)(curObj["StringFormat"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSupportsCallbackNull {
            get {
                if ((curObj["SupportsCallback"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Indication of whether the modem supports call-back")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool SupportsCallback {
            get {
                if ((curObj["SupportsCallback"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["SupportsCallback"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSupportsSynchronousConnectNull {
            get {
                if ((curObj["SupportsSynchronousConnect"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Boolean indicating that synchronous, as well as asynchronous, communication is su" +
            "pported.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool SupportsSynchronousConnect {
            get {
                if ((curObj["SupportsSynchronousConnect"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["SupportsSynchronousConnect"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The scoping System\'s CreationClassName.")]
        public string SystemCreationClassName {
            get {
                return ((string)(curObj["SystemCreationClassName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The scoping System\'s Name.")]
        public string SystemName {
            get {
                return ((string)(curObj["SystemName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Terminator property contains the string that marks the end of a command strin" +
            "g.\nExample: <cr>")]
        public string Terminator {
            get {
                return ((string)(curObj["Terminator"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTimeOfLastResetNull {
            get {
                if ((curObj["TimeOfLastReset"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The TimeOfLastReset property indicates the date and time this controller was last" +
            " reset.  This could mean the controller was powered down, or reinitialized.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.DateTime TimeOfLastReset {
            get {
                if ((curObj["TimeOfLastReset"] != null)) {
                    return ToDateTime(((string)(curObj["TimeOfLastReset"])));
                }
                else {
                    return System.DateTime.MinValue;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Tone property contains the command string that instructs the modem to use ton" +
            "e mode for dialing. The phone line must support tone dialing.\nExample: T.")]
        public string Tone {
            get {
                return ((string)(curObj["Tone"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The VoiceSwitchFeature property contains the command strings used to activate the" +
            " voice capabilities of a voice modem.\nExample: AT+V.")]
        public string VoiceSwitchFeature {
            get {
                return ((string)(curObj["VoiceSwitchFeature"]));
            }
        }
        
        private bool CheckIfProperClass(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions OptionsParam) {
            if (((path != null) 
                        && (string.Compare(path.ClassName, this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0))) {
                return true;
            }
            else {
                return CheckIfProperClass(new System.Management.ManagementObject(mgmtScope, path, OptionsParam));
            }
        }
        
        private bool CheckIfProperClass(System.Management.ManagementBaseObject theObj) {
            if (((theObj != null) 
                        && (string.Compare(((string)(theObj["__CLASS"])), this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0))) {
                return true;
            }
            else {
                System.Array parentClasses = ((System.Array)(theObj["__DERIVATION"]));
                if ((parentClasses != null)) {
                    int count = 0;
                    for (count = 0; (count < parentClasses.Length); count = (count + 1)) {
                        if ((string.Compare(((string)(parentClasses.GetValue(count))), this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        private bool ShouldSerializeAnswerMode() {
            if ((this.IsAnswerModeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeAvailability() {
            if ((this.IsAvailabilityNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeCompressionInfo() {
            if ((this.IsCompressionInfoNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeConfigManagerErrorCode() {
            if ((this.IsConfigManagerErrorCodeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeConfigManagerUserConfig() {
            if ((this.IsConfigManagerUserConfigNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeDialType() {
            if ((this.IsDialTypeNull == false)) {
                return true;
            }
            return false;
        }
        
        // Преобразует данные дату и время в формате DMTF в объект System.DateTime.
        static System.DateTime ToDateTime(string dmtfDate) {
            System.DateTime initializer = System.DateTime.MinValue;
            int year = initializer.Year;
            int month = initializer.Month;
            int day = initializer.Day;
            int hour = initializer.Hour;
            int minute = initializer.Minute;
            int second = initializer.Second;
            long ticks = 0;
            string dmtf = dmtfDate;
            System.DateTime datetime = System.DateTime.MinValue;
            string tempString = string.Empty;
            if ((dmtf == null)) {
                throw new System.ArgumentOutOfRangeException();
            }
            if ((dmtf.Length == 0)) {
                throw new System.ArgumentOutOfRangeException();
            }
            if ((dmtf.Length != 25)) {
                throw new System.ArgumentOutOfRangeException();
            }
            try {
                tempString = dmtf.Substring(0, 4);
                if (("****" != tempString)) {
                    year = int.Parse(tempString);
                }
                tempString = dmtf.Substring(4, 2);
                if (("**" != tempString)) {
                    month = int.Parse(tempString);
                }
                tempString = dmtf.Substring(6, 2);
                if (("**" != tempString)) {
                    day = int.Parse(tempString);
                }
                tempString = dmtf.Substring(8, 2);
                if (("**" != tempString)) {
                    hour = int.Parse(tempString);
                }
                tempString = dmtf.Substring(10, 2);
                if (("**" != tempString)) {
                    minute = int.Parse(tempString);
                }
                tempString = dmtf.Substring(12, 2);
                if (("**" != tempString)) {
                    second = int.Parse(tempString);
                }
                tempString = dmtf.Substring(15, 6);
                if (("******" != tempString)) {
                    ticks = (long.Parse(tempString) * ((long)((System.TimeSpan.TicksPerMillisecond / 1000))));
                }
                if (((((((((year < 0) 
                            || (month < 0)) 
                            || (day < 0)) 
                            || (hour < 0)) 
                            || (minute < 0)) 
                            || (minute < 0)) 
                            || (second < 0)) 
                            || (ticks < 0))) {
                    throw new System.ArgumentOutOfRangeException();
                }
            }
            catch (System.Exception e) {
                throw new System.ArgumentOutOfRangeException(null, e.Message);
            }
            datetime = new System.DateTime(year, month, day, hour, minute, second, 0);
            datetime = datetime.AddTicks(ticks);
            System.TimeSpan tickOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(datetime);
            int UTCOffset = 0;
            int OffsetToBeAdjusted = 0;
            long OffsetMins = ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)));
            tempString = dmtf.Substring(22, 3);
            if ((tempString != "******")) {
                tempString = dmtf.Substring(21, 4);
                try {
                    UTCOffset = int.Parse(tempString);
                }
                catch (System.Exception e) {
                    throw new System.ArgumentOutOfRangeException(null, e.Message);
                }
                OffsetToBeAdjusted = ((int)((OffsetMins - UTCOffset)));
                datetime = datetime.AddMinutes(((double)(OffsetToBeAdjusted)));
            }
            return datetime;
        }
        
        // Преобразует данный объект System.DateTime в формат даты и времени DMTF.
        static string ToDmtfDateTime(System.DateTime date) {
            string utcString = string.Empty;
            System.TimeSpan tickOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(date);
            long OffsetMins = ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)));
            if ((System.Math.Abs(OffsetMins) > 999)) {
                date = date.ToUniversalTime();
                utcString = "+000";
            }
            else {
                if ((tickOffset.Ticks >= 0)) {
                    utcString = string.Concat("+", ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute))).ToString().PadLeft(3, '0'));
                }
                else {
                    string strTemp = ((long)(OffsetMins)).ToString();
                    utcString = string.Concat("-", strTemp.Substring(1, (strTemp.Length - 1)).PadLeft(3, '0'));
                }
            }
            string dmtfDateTime = ((int)(date.Year)).ToString().PadLeft(4, '0');
            dmtfDateTime = string.Concat(dmtfDateTime, ((int)(date.Month)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((int)(date.Day)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((int)(date.Hour)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((int)(date.Minute)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((int)(date.Second)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ".");
            System.DateTime dtTemp = new System.DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
            long microsec = ((long)((((date.Ticks - dtTemp.Ticks) 
                        * 1000) 
                        / System.TimeSpan.TicksPerMillisecond)));
            string strMicrosec = ((long)(microsec)).ToString();
            if ((strMicrosec.Length > 6)) {
                strMicrosec = strMicrosec.Substring(0, 6);
            }
            dmtfDateTime = string.Concat(dmtfDateTime, strMicrosec.PadLeft(6, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, utcString);
            return dmtfDateTime;
        }
        
        private bool ShouldSerializeDriverDate() {
            if ((this.IsDriverDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeErrorCleared() {
            if ((this.IsErrorClearedNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeErrorControlInfo() {
            if ((this.IsErrorControlInfoNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeInactivityTimeout() {
            if ((this.IsInactivityTimeoutNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeIndex() {
            if ((this.IsIndexNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeInstallDate() {
            if ((this.IsInstallDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeLastErrorCode() {
            if ((this.IsLastErrorCodeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeMaxBaudRateToPhone() {
            if ((this.IsMaxBaudRateToPhoneNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeMaxBaudRateToSerialPort() {
            if ((this.IsMaxBaudRateToSerialPortNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeMaxNumberOfPasswords() {
            if ((this.IsMaxNumberOfPasswordsNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeModulationScheme() {
            if ((this.IsModulationSchemeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePowerManagementSupported() {
            if ((this.IsPowerManagementSupportedNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeRingsBeforeAnswer() {
            if ((this.IsRingsBeforeAnswerNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeSpeakerVolumeInfo() {
            if ((this.IsSpeakerVolumeInfoNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeStatusInfo() {
            if ((this.IsStatusInfoNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeSupportsCallback() {
            if ((this.IsSupportsCallbackNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeSupportsSynchronousConnect() {
            if ((this.IsSupportsSynchronousConnectNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeTimeOfLastReset() {
            if ((this.IsTimeOfLastResetNull == false)) {
                return true;
            }
            return false;
        }
        
        [Browsable(true)]
        public void CommitObject() {
            if ((isEmbedded == false)) {
                PrivateLateBoundObject.Put();
            }
        }
        
        [Browsable(true)]
        public void CommitObject(System.Management.PutOptions putOptions) {
            if ((isEmbedded == false)) {
                PrivateLateBoundObject.Put(putOptions);
            }
        }
        
        private void Initialize() {
            AutoCommitProp = true;
            isEmbedded = false;
        }
        
        private static string ConstructPath(string keyDeviceID) {
            string strPath = "root\\CimV2:Win32_POTSModem";
            strPath = string.Concat(strPath, string.Concat(".DeviceID=", string.Concat("\"", string.Concat(keyDeviceID, "\""))));
            return strPath;
        }
        
        private void InitializeObject(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            Initialize();
            if ((path != null)) {
                if ((CheckIfProperClass(mgmtScope, path, getOptions) != true)) {
                    throw new System.ArgumentException("Имя класса не совпадает.");
                }
            }
            PrivateLateBoundObject = new System.Management.ManagementObject(mgmtScope, path, getOptions);
            PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
            curObj = PrivateLateBoundObject;
        }
        
        // Различные перезагрузки GetInstances(), которые помогают в перечислении экземпляров класса WMI.
        public static POTSModemCollection GetInstances() {
            return GetInstances(null, null, null);
        }
        
        public static POTSModemCollection GetInstances(string condition) {
            return GetInstances(null, condition, null);
        }
        
        public static POTSModemCollection GetInstances(string[] selectedProperties) {
            return GetInstances(null, null, selectedProperties);
        }
        
        public static POTSModemCollection GetInstances(string condition, string[] selectedProperties) {
            return GetInstances(null, condition, selectedProperties);
        }
        
        public static POTSModemCollection GetInstances(System.Management.ManagementScope mgmtScope, System.Management.EnumerationOptions enumOptions) {
            if ((mgmtScope == null)) {
                if ((statMgmtScope == null)) {
                    mgmtScope = new System.Management.ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\CimV2";
                }
                else {
                    mgmtScope = statMgmtScope;
                }
            }
            System.Management.ManagementPath pathObj = new System.Management.ManagementPath();
            pathObj.ClassName = "Win32_POTSModem";
            pathObj.NamespacePath = "root\\CimV2";
            System.Management.ManagementClass clsObject = new System.Management.ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null)) {
                enumOptions = new System.Management.EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new POTSModemCollection(clsObject.GetInstances(enumOptions));
        }
        
        public static POTSModemCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition) {
            return GetInstances(mgmtScope, condition, null);
        }
        
        public static POTSModemCollection GetInstances(System.Management.ManagementScope mgmtScope, string[] selectedProperties) {
            return GetInstances(mgmtScope, null, selectedProperties);
        }
        
        public static POTSModemCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition, string[] selectedProperties) {
            if ((mgmtScope == null)) {
                if ((statMgmtScope == null)) {
                    mgmtScope = new System.Management.ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\CimV2";
                }
                else {
                    mgmtScope = statMgmtScope;
                }
            }
            System.Management.ManagementObjectSearcher ObjectSearcher = new System.Management.ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_POTSModem", condition, selectedProperties));
            System.Management.EnumerationOptions enumOptions = new System.Management.EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            ObjectSearcher.Options = enumOptions;
            return new POTSModemCollection(ObjectSearcher.Get());
        }
        
        [Browsable(true)]
        public static POTSModem CreateInstance() {
            System.Management.ManagementScope mgmtScope = null;
            if ((statMgmtScope == null)) {
                mgmtScope = new System.Management.ManagementScope();
                mgmtScope.Path.NamespacePath = CreatedWmiNamespace;
            }
            else {
                mgmtScope = statMgmtScope;
            }
            System.Management.ManagementPath mgmtPath = new System.Management.ManagementPath(CreatedClassName);
            System.Management.ManagementClass tmpMgmtClass = new System.Management.ManagementClass(mgmtScope, mgmtPath, null);
            return new POTSModem(tmpMgmtClass.CreateInstance());
        }
        
        [Browsable(true)]
        public void Delete() {
            PrivateLateBoundObject.Delete();
        }
        
        public uint Reset0() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Reset", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public uint SetPowerState(ushort PowerState, System.DateTime Time) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("SetPowerState");
                inParams["PowerState"] = ((ushort)(PowerState));
                inParams["Time"] = ToDmtfDateTime(((System.DateTime)(Time)));
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("SetPowerState", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public enum AnswerModeValues {
            
            Unknown0 = 0,
            
            Other0 = 1,
            
            Disabled = 2,
            
            Manual_Answer = 3,
            
            Auto_Answer = 4,
            
            Auto_Answer_with_Call_Back = 5,
            
            NULL_ENUM_VALUE = 6,
        }
        
        public enum AvailabilityValues {
            
            Other0 = 1,
            
            Unknown0 = 2,
            
            Running_Full_Power = 3,
            
            Warning = 4,
            
            In_Test = 5,
            
            Not_Applicable = 6,
            
            Power_Off = 7,
            
            Off_Line = 8,
            
            Off_Duty = 9,
            
            Degraded = 10,
            
            Not_Installed = 11,
            
            Install_Error = 12,
            
            Power_Save_Unknown = 13,
            
            Power_Save_Low_Power_Mode = 14,
            
            Power_Save_Standby = 15,
            
            Power_Cycle = 16,
            
            Power_Save_Warning = 17,
            
            Paused = 18,
            
            Not_Ready = 19,
            
            Not_Configured = 20,
            
            Quiesced = 21,
            
            NULL_ENUM_VALUE = 0,
        }
        
        public enum CompressionInfoValues {
            
            Unknown0 = 0,
            
            Other0 = 1,
            
            No_Compression = 2,
            
            MNP_5 = 3,
            
            V_42bis = 4,
            
            NULL_ENUM_VALUE = 5,
        }
        
        public enum ConfigManagerErrorCodeValues {
            
            This_device_is_working_properly_ = 0,
            
            This_device_is_not_configured_correctly_ = 1,
            
            Windows_cannot_load_the_driver_for_this_device_ = 2,
            
            The_driver_for_this_device_might_be_corrupted_or_your_system_may_be_running_low_on_memory_or_other_resources_ = 3,
            
            This_device_is_not_working_properly_One_of_its_drivers_or_your_registry_might_be_corrupted_ = 4,
            
            The_driver_for_this_device_needs_a_resource_that_Windows_cannot_manage_ = 5,
            
            The_boot_configuration_for_this_device_conflicts_with_other_devices_ = 6,
            
            Cannot_filter_ = 7,
            
            The_driver_loader_for_the_device_is_missing_ = 8,
            
            This_device_is_not_working_properly_because_the_controlling_firmware_is_reporting_the_resources_for_the_device_incorrectly_ = 9,
            
            This_device_cannot_start_ = 10,
            
            This_device_failed_ = 11,
            
            This_device_cannot_find_enough_free_resources_that_it_can_use_ = 12,
            
            Windows_cannot_verify_this_device_s_resources_ = 13,
            
            This_device_cannot_work_properly_until_you_restart_your_computer_ = 14,
            
            This_device_is_not_working_properly_because_there_is_probably_a_re_enumeration_problem_ = 15,
            
            Windows_cannot_identify_all_the_resources_this_device_uses_ = 16,
            
            This_device_is_asking_for_an_unknown_resource_type_ = 17,
            
            Reinstall_the_drivers_for_this_device_ = 18,
            
            Failure_using_the_VxD_loader_ = 19,
            
            Your_registry_might_be_corrupted_ = 20,
            
            System_failure_Try_changing_the_driver_for_this_device_If_that_does_not_work_see_your_hardware_documentation_Windows_is_removing_this_device_ = 21,
            
            This_device_is_disabled_ = 22,
            
            System_failure_Try_changing_the_driver_for_this_device_If_that_doesn_t_work_see_your_hardware_documentation_ = 23,
            
            This_device_is_not_present_is_not_working_properly_or_does_not_have_all_its_drivers_installed_ = 24,
            
            Windows_is_still_setting_up_this_device_ = 25,
            
            Windows_is_still_setting_up_this_device_0 = 26,
            
            This_device_does_not_have_valid_log_configuration_ = 27,
            
            The_drivers_for_this_device_are_not_installed_ = 28,
            
            This_device_is_disabled_because_the_firmware_of_the_device_did_not_give_it_the_required_resources_ = 29,
            
            This_device_is_using_an_Interrupt_Request_IRQ_resource_that_another_device_is_using_ = 30,
            
            This_device_is_not_working_properly_because_Windows_cannot_load_the_drivers_required_for_this_device_ = 31,
            
            NULL_ENUM_VALUE = 32,
        }
        
        public enum DialTypeValues {
            
            Unknown0 = 0,
            
            Tone0 = 1,
            
            Pulse0 = 2,
            
            NULL_ENUM_VALUE = 3,
        }
        
        public enum ErrorControlInfoValues {
            
            Unknown0 = 0,
            
            Other0 = 1,
            
            No_Error_Correction = 2,
            
            MNP_4 = 3,
            
            LAPM = 4,
            
            NULL_ENUM_VALUE = 5,
        }
        
        public enum ModulationSchemeValues {
            
            Unknown0 = 0,
            
            Other0 = 1,
            
            Not_Supported = 2,
            
            Bell_103 = 3,
            
            Bell_212A = 4,
            
            V_22bis = 5,
            
            V_32 = 6,
            
            V_32bis = 7,
            
            V_turbo = 8,
            
            V_FC = 9,
            
            V_34 = 10,
            
            V_34bis = 11,
            
            NULL_ENUM_VALUE = 12,
        }
        
        public enum PowerManagementCapabilitiesValues {
            
            Unknown0 = 0,
            
            Not_Supported = 1,
            
            Disabled = 2,
            
            Enabled = 3,
            
            Power_Saving_Modes_Entered_Automatically = 4,
            
            Power_State_Settable = 5,
            
            Power_Cycling_Supported = 6,
            
            Timed_Power_On_Supported = 7,
            
            NULL_ENUM_VALUE = 8,
        }
        
        public enum SpeakerVolumeInfoValues {
            
            Unknown0 = 0,
            
            Other0 = 1,
            
            Not_Supported = 2,
            
            High = 3,
            
            Medium = 4,
            
            Low = 5,
            
            Off = 6,
            
            Auto = 7,
            
            NULL_ENUM_VALUE = 8,
        }
        
        public enum StatusInfoValues {
            
            Other0 = 1,
            
            Unknown0 = 2,
            
            Enabled = 3,
            
            Disabled = 4,
            
            Not_Applicable = 5,
            
            NULL_ENUM_VALUE = 0,
        }
        
        // Реализация перечислителя для перечисления экземпляров класса.
        public class POTSModemCollection : object, ICollection {
            
            private ManagementObjectCollection privColObj;
            
            public POTSModemCollection(ManagementObjectCollection objCollection) {
                privColObj = objCollection;
            }
            
            public virtual int Count {
                get {
                    return privColObj.Count;
                }
            }
            
            public virtual bool IsSynchronized {
                get {
                    return privColObj.IsSynchronized;
                }
            }
            
            public virtual object SyncRoot {
                get {
                    return this;
                }
            }
            
            public virtual void CopyTo(System.Array array, int index) {
                privColObj.CopyTo(array, index);
                int nCtr;
                for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1)) {
                    array.SetValue(new POTSModem(((System.Management.ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }
            
            public virtual System.Collections.IEnumerator GetEnumerator() {
                return new POTSModemEnumerator(privColObj.GetEnumerator());
            }
            
            public class POTSModemEnumerator : object, System.Collections.IEnumerator {
                
                private ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;
                
                public POTSModemEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum) {
                    privObjEnum = objEnum;
                }
                
                public virtual object Current {
                    get {
                        return new POTSModem(((System.Management.ManagementObject)(privObjEnum.Current)));
                    }
                }
                
                public virtual bool MoveNext() {
                    return privObjEnum.MoveNext();
                }
                
                public virtual void Reset() {
                    privObjEnum.Reset();
                }
            }
        }
        
        // TypeConverter для обработки нулевых значений свойств ValueType
        public class WMIValueTypeConverter : TypeConverter {
            
            private TypeConverter baseConverter;
            
            private System.Type baseType;
            
            public WMIValueTypeConverter(System.Type inBaseType) {
                baseConverter = TypeDescriptor.GetConverter(inBaseType);
                baseType = inBaseType;
            }
            
            public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type srcType) {
                return baseConverter.CanConvertFrom(context, srcType);
            }
            
            public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType) {
                return baseConverter.CanConvertTo(context, destinationType);
            }
            
            public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                return baseConverter.ConvertFrom(context, culture, value);
            }
            
            public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary dictionary) {
                return baseConverter.CreateInstance(context, dictionary);
            }
            
            public override bool GetCreateInstanceSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetCreateInstanceSupported(context);
            }
            
            public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, System.Attribute[] attributeVar) {
                return baseConverter.GetProperties(context, value, attributeVar);
            }
            
            public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetPropertiesSupported(context);
            }
            
            public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValues(context);
            }
            
            public override bool GetStandardValuesExclusive(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValuesExclusive(context);
            }
            
            public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValuesSupported(context);
            }
            
            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType) {
                if ((baseType.BaseType == typeof(System.Enum))) {
                    if ((value.GetType() == destinationType)) {
                        return value;
                    }
                    if ((((value == null) 
                                && (context != null)) 
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                        return  "NULL_ENUM_VALUE" ;
                    }
                    return baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((baseType == typeof(bool)) 
                            && (baseType.BaseType == typeof(System.ValueType)))) {
                    if ((((value == null) 
                                && (context != null)) 
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                        return "";
                    }
                    return baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((context != null) 
                            && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                    return "";
                }
                return baseConverter.ConvertTo(context, culture, value, destinationType);
            }
        }
        
        // Встроенный класс, отражающий свойства системы WMI.
        [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ManagementSystemProperties {
            
            private System.Management.ManagementBaseObject PrivateLateBoundObject;
            
            public ManagementSystemProperties(System.Management.ManagementBaseObject ManagedObject) {
                PrivateLateBoundObject = ManagedObject;
            }
            
            [Browsable(true)]
            public int GENUS {
                get {
                    return ((int)(PrivateLateBoundObject["__GENUS"]));
                }
            }
            
            [Browsable(true)]
            public string CLASS {
                get {
                    return ((string)(PrivateLateBoundObject["__CLASS"]));
                }
            }
            
            [Browsable(true)]
            public string SUPERCLASS {
                get {
                    return ((string)(PrivateLateBoundObject["__SUPERCLASS"]));
                }
            }
            
            [Browsable(true)]
            public string DYNASTY {
                get {
                    return ((string)(PrivateLateBoundObject["__DYNASTY"]));
                }
            }
            
            [Browsable(true)]
            public string RELPATH {
                get {
                    return ((string)(PrivateLateBoundObject["__RELPATH"]));
                }
            }
            
            [Browsable(true)]
            public int PROPERTY_COUNT {
                get {
                    return ((int)(PrivateLateBoundObject["__PROPERTY_COUNT"]));
                }
            }
            
            [Browsable(true)]
            public string[] DERIVATION {
                get {
                    return ((string[])(PrivateLateBoundObject["__DERIVATION"]));
                }
            }
            
            [Browsable(true)]
            public string SERVER {
                get {
                    return ((string)(PrivateLateBoundObject["__SERVER"]));
                }
            }
            
            [Browsable(true)]
            public string NAMESPACE {
                get {
                    return ((string)(PrivateLateBoundObject["__NAMESPACE"]));
                }
            }
            
            [Browsable(true)]
            public string PATH {
                get {
                    return ((string)(PrivateLateBoundObject["__PATH"]));
                }
            }
        }
    }
}
