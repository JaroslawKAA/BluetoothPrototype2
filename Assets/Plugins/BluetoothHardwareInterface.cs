#define EXPERIMENTAL_MACOS_EDITOR
/*

This build includes an experimental implementation for the macOS editor of Unity
It is experiemental because of the way that the Unity editor hangs on to plugin
instances after leaving play mode. This causes this plugin to not free up its
resources and therefore can cause crashes in the Unity editor on macOS.

Since Unity does not give plugins or apps a chance to do anything when the user
hits the play / stop button in the Editor there isn't a chance for the app to
deinitialize this plugin.

What I have found in my own use of this is that if you put a button on your app
somewhere that you can press before hitting the stop button in the editor and
then in that button handler call this plugin's Deinitialize method it seems to
minimize how often the editor crashes.

WARNING: using the macOS editor can cause the editor to crash an loose your work
and settings. Save often. You have been warned, so please don't contact me if
you have lost work becausee of this problem. This is experimental only. Use at
your own risk.

*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#if UNITY_2018_3_OR_NEWER
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#endif

public enum BluetoothDeviceRole
{
    Central,
    Peripheral,
    Both
}

public class BluetoothLEHardwareInterface
{
    public enum CBCharacteristicProperties
    {
        CBCharacteristicPropertyBroadcast = 0x01,
        CBCharacteristicPropertyRead = 0x02,
        CBCharacteristicPropertyWriteWithoutResponse = 0x04,
        CBCharacteristicPropertyWrite = 0x08,
        CBCharacteristicPropertyNotify = 0x10,
        CBCharacteristicPropertyIndicate = 0x20,
        CBCharacteristicPropertyAuthenticatedSignedWrites = 0x40,
        CBCharacteristicPropertyExtendedProperties = 0x80,
        CBCharacteristicPropertyNotifyEncryptionRequired = 0x100,
        CBCharacteristicPropertyIndicateEncryptionRequired = 0x200,
    };

    public enum ScanMode
    {
        LowPower = 0,
        Balanced = 1,
        LowLatency = 2
    }

    public enum ConnectionPriority
    {
        LowPower = 0,
        Balanced = 1,
        High = 2,
    }

    public enum AdvertisingMode
    {
        LowPower = 0,
        Balanced = 1,
        LowLatency = 2
    }

    public enum AdvertisingPower
    {
        UltraLow = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }

    public enum iOSProximity
    {
        Unknown = 0,
        Immediate = 1,
        Near = 2,
        Far = 3,
    }

    public struct iBeaconData
    {
        public string UUID;
        public int Major;
        public int Minor;
        public int RSSI;
        public int AndroidSignalPower;
        public iOSProximity iOSProximity;
    }

#if UNITY_ANDROID
    public enum CBAttributePermissions
    {
        CBAttributePermissionsReadable = 0x01,
        CBAttributePermissionsWriteable = 0x10,
        CBAttributePermissionsReadEncryptionRequired = 0x02,
        CBAttributePermissionsWriteEncryptionRequired = 0x20,
    };
#else
	public  enum CBAttributePermissions
	{
		CBAttributePermissionsReadable = 0x01,
		CBAttributePermissionsWriteable = 0x02,
		CBAttributePermissionsReadEncryptionRequired = 0x04,
		CBAttributePermissionsWriteEncryptionRequired = 0x08,
	};
#endif

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
	public delegate void UnitySendMessageCallbackDelegate (IntPtr objectName, IntPtr commandName, IntPtr commandData);

	[DllImport ("BluetoothLEOSX")]
	private static extern void ConnectUnitySendMessageCallback ([MarshalAs (UnmanagedType.FunctionPtr)]UnitySendMessageCallbackDelegate callbackMethod);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLELog (string message);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEInitialize ([MarshalAs (UnmanagedType.Bool)]bool asCentral, [MarshalAs (UnmanagedType.Bool)]bool asPeripheral);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEDeInitialize ();

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEPauseMessages (bool isPaused);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEStopScan ();

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEConnectToPeripheral (string name);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEDisconnectAll ();

	[DllImport("BluetoothLEOSX")]
	private static extern void OSXBluetoothLERequestMtu (string name, int mtu);

	[DllImport("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEReadRSSI (string name);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEDisconnectPeripheral (string name);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("BluetoothLEOSX")]
	private static extern void OSXBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

#endif

#if UNITY_IOS || UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLELog (string message);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEInitialize (bool asCentral, bool asPeripheral);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDeInitialize ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPauseMessages (bool isPaused);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopScan ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEConnectToPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectAll ();

	[DllImport("__Internal")]
	private static extern void _iOSBluetoothLERequestMtu(string name, int mtu);

	[DllImport("__Internal")]
	private static extern void _iOSBluetoothLEReadRSSI(string name);

#if !UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForBeacons (string proximityUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopBeaconScan ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPeripheralName (string newName);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateService (string uuid, bool primary);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveService (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveServices ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateCharacteristic (string uuid, int properties, int permissions, byte[] data, int length);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristic (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristics ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStartAdvertising ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopAdvertising ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUpdateCharacteristicValue (string uuid, byte[] data, int length);
#endif
#elif UNITY_ANDROID
    static AndroidJavaObject _android = null;
#endif


    private static BluetoothDeviceScript bluetoothDeviceScript;

    public static void Log(string message)
    {
#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		Debug.Log(message);
#else
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
            if (_android == null)
            {
                AndroidJavaClass javaClass =
                    new AndroidJavaClass("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
                _android = javaClass.CallStatic<AndroidJavaObject>("getInstance");
            }

            if (_android != null)
                _android.Call("androidBluetoothLog", message);
#endif
        }
#endif
    }

    /// <summary>
    /// Initialize the Bluetooth system as either a central, 
    /// peripheral or both. Acting as a peripheral is only 
    /// available for iOS.
    /// When completed the onInitialized callback will be executed. 
    /// If there is an error the onCantInit callback will be 
    /// executed.
    /// </summary>
    /// <param name="bluetoothDeviceRole">Role of device.</param>
    /// <param name="onInitialized">Callback</param>
    /// <param name="onCantInit">Error callback</param>
    /// <returns></returns>
    public static BluetoothDeviceScript Initialize(BluetoothDeviceRole bluetoothDeviceRole, Action onInitialized,
        Action<string> onCantInit)
    {
        bluetoothDeviceScript = null;

#if UNITY_2018_3_OR_NEWER
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            Permission.RequestUserPermission(Permission.FineLocation);
#endif
#endif

        GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
        if (bluetoothLEReceiver == null)
            bluetoothLEReceiver = new GameObject("BluetoothLEReceiver");

        if (bluetoothLEReceiver != null)
        {
            bluetoothDeviceScript = bluetoothLEReceiver.GetComponent<BluetoothDeviceScript>();
            if (bluetoothDeviceScript == null)
                bluetoothDeviceScript = bluetoothLEReceiver.AddComponent<BluetoothDeviceScript>();

            if (bluetoothDeviceScript != null)
            {
                bluetoothDeviceScript.InitializedAction = onInitialized;
                bluetoothDeviceScript.ErrorAction = onCantInit;
            }
        }

        GameObject.DontDestroyOnLoad(bluetoothLEReceiver);

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		ConnectUnitySendMessageCallback ((objectName, commandName, commandData) => {
			string name = Marshal.PtrToStringAuto (objectName);
			string command = Marshal.PtrToStringAuto (commandName);
			string data = Marshal.PtrToStringAuto (commandData);

			GameObject foundObject = GameObject.Find (name);
			if (foundObject != null)
				foundObject.SendMessage (command, data);
		});

		switch (bluetoothDeviceRole)
			{
				case BluetoothDeviceRole.Central:
					BluetoothLEHardwareInterface.OSXBluetoothLEInitialize(true, false);
					break;
				case BluetoothDeviceRole.Peripheral:
					BluetoothLEHardwareInterface.OSXBluetoothLEInitialize(false, true);
					break;
				case BluetoothDeviceRole.Both:
					BluetoothLEHardwareInterface.OSXBluetoothLEInitialize(true, true);
					break;
			}
#else
        if (Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.SendMessage("OnBluetoothMessage", "Initialized");
        }
        else
        {
#if UNITY_IOS || UNITY_TVOS
        switch (bluetoothDeviceRole)
			{
				case BluetoothDeviceRole.Central:
					_iOSBluetoothLEInitialize (true, false);
					break;
				case BluetoothDeviceRole.Peripheral:
					_iOSBluetoothLEInitialize (false, true);
					break;
				case BluetoothDeviceRole.Both:
					_iOSBluetoothLEInitialize (true, true);
					break;
			}
#elif UNITY_ANDROID
            if (_android == null)
            {
                AndroidJavaClass javaClass =
                    new AndroidJavaClass("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
                _android = javaClass.CallStatic<AndroidJavaObject>("getInstance");
            }

            if (_android != null)
            {
                switch (bluetoothDeviceRole)
                {
                    case BluetoothDeviceRole.Central:
                        _android.Call("androidBluetoothInitialize", true, false);
                        break;
                    case BluetoothDeviceRole.Peripheral:
                        _android.Call("androidBluetoothInitialize", false, true);
                        break;
                    case BluetoothDeviceRole.Both:
                        _android.Call("androidBluetoothInitialize", true, true);
                        break;
                }
            }
#endif
        }
#endif

        return bluetoothDeviceScript;
    }

    /// <summary>
    /// DeInitialize the Bluetooth system.
    /// When completed the action callback will be executed.
    /// </summary>
    /// <param name="callBack">DeInitialized callback.</param>
    public static void DeInitialize(Action callBack)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DeinitializedAction = callBack;

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		BluetoothLEHardwareInterface.OSXBluetoothLEDeInitialize ();
#else
        if (Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.SendMessage("OnBluetoothMessage", "DeInitialized");
        }
        else
        {
#if UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEDeInitialize ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothDeInitialize");
#endif
        }
#endif
    }

    /// <summary>
    /// This method is automatically called by the 
    /// BluetoothDeviceScript when it has been notified by the 
    /// Java code that everything else has been deinitialized.
    /// </summary>
    public static void FinishDeInitialize()
    {
        GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
        if (bluetoothLEReceiver != null)
            GameObject.Destroy(bluetoothLEReceiver);
    }

    public static void BluetoothEnable(bool enable)
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
			//_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothEnable", enable);
#endif
        }
    }

    /// <summary>
    /// This method sets the Android scan mode. It affects the 
    /// speed scanning is done and battery. The faster the 
    /// scanning (lower latency) the more battery power. This 
    /// method does nothing on iOS.
    /// </summary>
    /// <param name="scanMode"></param>
    public static void BluetoothScanMode(ScanMode scanMode)
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothScanMode", (int) scanMode);
#endif
        }
    }

    /// <summary>
    /// This method sets the Android connection priority. It 
    /// affects how fast data is passed back and forth with a 
    /// connected peripheral. This method will use more 
    /// battery at higher settings. This method does nothing 
    /// on iOS.
    /// </summary>
    /// <param name="connectionPriority"></param>
    public static void BluetoothConnectionPriority(ConnectionPriority connectionPriority)
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothConnectionPriority", (int) connectionPriority);
#endif
        }
    }

    public static void BluetoothAdvertisingMode(AdvertisingMode advertisingMode)
    {
        if (!Application.isEditor)
        {
#if UNITY_IPHONE || UNITY_TVOS
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothAdvertisingMode", (int) advertisingMode);
#endif
        }
    }

    public static void BluetoothAdvertisingPower(AdvertisingPower advertisingPower)
    {
        if (!Application.isEditor)
        {
#if UNITY_IPHONE || UNITY_TVOS
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothAdvertisingPower", (int) advertisingPower);
#endif
        }
    }

    /// <summary>
    /// This method notifies the bluetooth system that the app 
    /// is going to be paused or unpaused.
    /// </summary>
    /// <param name="isPaused"></param>
    public static void PauseMessages(bool isPaused)
    {
#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		OSXBluetoothLEPauseMessages (isPaused);
#else
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEPauseMessages (isPaused);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothPause", isPaused);
#endif
        }
#endif
    }

    /// <summary>
    /// This method starts scanning for the list of proximity 
    /// uuids. This list of uuids is required. Only iBeacons 
    /// are scanned for. The response action is called with 
    /// the iBeaconData structure shown above. The 
    /// iOSProximity is always “unknown” on Android. There is 
    /// an example of calculating the distance using the 
    /// AndroidSignalPower and RSSI value in the 
    /// iBeaconExample.
    /// </summary>
    /// <param name="proximityUUIDs">Scanning for beacons requires that you know the Proximity UUID.</param>
    /// <param name="actionBeaconResponse"></param>
    public static void ScanForBeacons(string[] proximityUUIDs, Action<iBeaconData> actionBeaconResponse)
    {
        if (proximityUUIDs != null && proximityUUIDs.Length >= 0)
        {
            if (!Application.isEditor)
            {
                if (bluetoothDeviceScript != null)
                    bluetoothDeviceScript.DiscoveredBeaconAction = actionBeaconResponse;

                string proximityUUIDsString = null;

                if (proximityUUIDs != null && proximityUUIDs.Length > 0)
                {
                    proximityUUIDsString = "";

                    foreach (string proximityUUID in proximityUUIDs)
                        proximityUUIDsString += proximityUUID + "|";

                    proximityUUIDsString = proximityUUIDsString.Substring(0, proximityUUIDsString.Length - 1);
                }

#if UNITY_IOS
				_iOSBluetoothLEScanForBeacons (proximityUUIDsString);
#elif UNITY_ANDROID
                if (_android != null)
                    _android.Call("androidBluetoothScanForBeacons", proximityUUIDsString);
#endif
            }
        }
    }

    /// <summary>
    /// This method will request a new MTU value. The max for 
    /// most iOS devices is 184.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="mtu"></param>
    /// <param name="action"></param>
    public static void RequestMtu(string name, int mtu, Action<string, int> action)
    {
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.RequestMtuAction = action;
        }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		if (mtu > 184)
			mtu = 184;
		OSXBluetoothLERequestMtu(name, mtu);
#elif UNITY_IOS || UNITY_TVOS
        if (mtu > 180)
            mtu = 180;
	    _iOSBluetoothLERequestMtu (name, mtu);
#elif UNITY_ANDROID
        if (_android != null)
        {
            _android.Call("androidBluetoothRequestMtu", name, mtu);
        }
#endif
    }

    /// <summary>
    /// This method will read the RSSI value when you are 
    /// already connected to a device.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public static void ReadRSSI(string name, Action<string, int> action)
    {
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.ReadRSSIAction = action;
        }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		OSXBluetoothLEReadRSSI(name);
#elif UNITY_IOS || UNITY_TVOS
		_iOSBluetoothLEReadRSSI(name);
#elif UNITY_ANDROID
        if (_android != null)
        {
            _android.Call("androidBluetoothReadRSSI", name);
        }
#endif
    }

    /// <summary>
    ///     This method puts the device into a scan mode looking 
    /// for any peripherals that support the service UUIDs in 
    /// the serviceUUIDs parameter array. If serviceUUIDs is 
    /// NULL all Bluetooth LE peripherals will be discovered. 
    ///     As devices are discovered the action callback will be 
    /// called with the ID and name of the peripheral.
    ///    The default value for the actionAdvertisingInfo 
    /// callback is null for backwards compatibility. If you 
    /// supply a callback for this parameter it will be called 
    /// each time advertising data is received from a device. 
    /// You will receive the ID and address of the device, the
    /// 
    /// RSSI and the manufacturer specific data from the 
    /// advertising packet. The rssiOnly parameter will allow scanned devices that 
    /// don’t have manufacturer specific data to still send 
    /// the RSSI value. The reason this defaults to false is 
    /// for backwards compatibility.
    ///     The clearPeripheralList is only used in iOS, but is 
    /// here for cross platform compatibility in the api.
    /// </summary>
    /// <param name="serviceUUIDs">Services that you are looking for.</param>
    /// <param name="onDeviceDetected">Callback witch returning: device name, mac address.</param>
    /// <param name="actionAdvertisingInfo">Callback witch returning advertising info: device name, mac address, rssi strength, manufacturer specific data</param>
    /// <param name="rssiOnly"></param>
    /// <param name="clearPeripheralList">Only for iOS devices. Its true for cross platform compatibility.</param>
    /// <param name="recordType"></param>
    public static void ScanForPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> onDeviceDetected,
        Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false,
        bool clearPeripheralList = true, int recordType = 0xFF)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                bluetoothDeviceScript.DiscoveredPeripheralAction = onDeviceDetected;
                bluetoothDeviceScript.DiscoveredPeripheralWithAdvertisingInfoAction = actionAdvertisingInfo;

                if (bluetoothDeviceScript.DiscoveredDeviceList != null)
                    bluetoothDeviceScript.DiscoveredDeviceList.Clear();
            }

            string serviceUUIDsString = null;

            if (serviceUUIDs != null && serviceUUIDs.Length > 0)
            {
                serviceUUIDsString = "";

                foreach (string serviceUUID in serviceUUIDs)
                    serviceUUIDsString += serviceUUID + "|";

                serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);
            }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
#elif UNITY_ANDROID
            if (_android != null)
            {
                if (serviceUUIDsString == null)
                    serviceUUIDsString = "";

                _android.Call("androidBluetoothScanForPeripheralsWithServices", serviceUUIDsString, rssiOnly,
                    recordType);
            }
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method will retrieve a list of all currently 
    /// connected peripherals with the UUIDs listed in the
    /// serviceUUIDs parameter. If serviceUUIDs is NULL all 
    /// Bluetooth LE peripherals will be discovered. As 
    /// devices are discovered the action callback will be 
    /// called with the ID and name of the peripheral.
    /// </summary>
    /// <param name="serviceUUIDs">Services you are looking for.</param>
    /// <param name="onDeviceRead">Callback invoked when device is returned. Callback contain: ...</param>
    public static void RetrieveListOfPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> onDeviceRead)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                bluetoothDeviceScript.RetrievedConnectedPeripheralAction = onDeviceRead;

                if (bluetoothDeviceScript.DiscoveredDeviceList != null)
                    bluetoothDeviceScript.DiscoveredDeviceList.Clear();
            }

            string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;

            foreach (string serviceUUID in serviceUUIDs)
                serviceUUIDsString += serviceUUID + "|";

            // strip the last delimeter
            serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothRetrieveListOfPeripheralsWithServices", serviceUUIDsString);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method stops the scanning mode initiated using 
    /// the ScanForPeripheralsWithServices method call.
    /// </summary>
    public static void StopScan()
    {
#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		OSXBluetoothLEStopScan ();
#else
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEStopScan ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothStopScan");
#endif
        }
#endif
    }

    /// <summary>
    /// This method stops the scanning for beacons initiated 
    /// using the ScanForBeacons method call.
    /// </summary>
    public static void StopBeaconScan()
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS
			_iOSBluetoothLEStopBeaconScan ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothStopBeaconScan");
#endif
        }
    }

    public static void DisconnectAll()
    {
#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
		OSXBluetoothLEDisconnectAll ();
#else
        if (!Application.isEditor)
        {
#if UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEDisconnectAll ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothDisconnectAll");
#endif
        }
#endif
    }

    /// <summary>
    /// This method attempts to connect to the named 
    /// peripheral. If the connection is successful the 
    /// connectAction will be called with the name of the 
    /// peripheral connected to. Once connected the 
    /// serviceAction is called for each service the 
    /// peripheral supports. Each service is enumerated and 
    /// the characteristics supported by each service are 
    /// indicated by calling the characteristicAction 
    /// callback.
    /// The default value for the disconnectAction is null for 
    /// backwards compatibility. If you supply a callback for 
    /// this parameter it will be called whenever the 
    /// connected device disconnects. Keep in mind that if you 
    /// also supply a callback for the DisconnectPeripheral 
    /// command below both callbacks will be called.
    /// </summary>
    /// <param name="macAddress"></param>
    /// <param name="onConnected"></param>
    /// <param name="onServiceEnumeration"></param>
    /// <param name="onCharacteristicEnumeration"></param>
    /// <param name="onDisconnected"></param>
    public static void ConnectToPeripheral(string macAddress, Action<string> onConnected,
        Action<string, string> onServiceEnumeration, Action<string, string, string> onCharacteristicEnumeration,
        Action<string> onDisconnected = null)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                bluetoothDeviceScript.ConnectedPeripheralAction = onConnected;
                bluetoothDeviceScript.DiscoveredServiceAction = onServiceEnumeration;
                bluetoothDeviceScript.DiscoveredCharacteristicAction = onCharacteristicEnumeration;
                bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = onDisconnected;
            }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLEConnectToPeripheral (name);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEConnectToPeripheral (name);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothConnectToPeripheral", macAddress);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method will disconnect a peripheral by name. When 
    /// the disconnection is complete the action callback is 
    /// called with the ID of the peripheral.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public static void DisconnectPeripheral(string name, Action<string> action)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.DisconnectedPeripheralAction = action;

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLEDisconnectPeripheral (name);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEDisconnectPeripheral (name);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidBluetoothDisconnectPeripheral", name);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method will initiate a read of a characteristic 
    /// using the name of the peripheral, the service and 
    /// characteristic to be read. If the read is successful 
    /// the action callback is called with the UUID of the 
    /// characteristic and the raw bytes of the read.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="service"></param>
    /// <param name="characteristic"></param>
    /// <param name="action"></param>
    public static void ReadCharacteristic(string name, string service, string characteristic,
        Action<string, byte[]> action)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] =
                        new Dictionary<string, Action<string, byte[]>>();

#if UNITY_IOS || UNITY_TVOS || (EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][FullUUID(characteristic).ToLower()] =
                    action;
#endif
            }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLEReadCharacteristic (name, service, characteristic);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEReadCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidReadCharacteristic", name, service, characteristic);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method will initiate a write of a characteristic 
    /// by the name of the peripheral and the service and 
    /// characteristic to be written. The value to write is a 
    /// byte buffer with the length indicated in the data and 
    /// length parameters. The withResponse parameter 
    /// indicates when the user wants a response after the 
    /// write is completed. If a response is requested then 
    /// the action callback is called with the message from 
    /// the Bluetooth system on the result of the write 
    /// operation.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="service"></param>
    /// <param name="characteristic"></param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <param name="withResponse"></param>
    /// <param name="action"></param>
    public static void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length,
        bool withResponse, Action<string> action)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.DidWriteCharacteristicAction = action;

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
    		OSXBluetoothLEWriteCharacteristic(name, service, characteristic, data, length, withResponse);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method will subscribe to a characteristic by 
    /// peripheral name and the service and characteristic. 
    /// The notificationAction callback is called when the 
    /// notification occurs and the action callback is called 
    /// whenever the characteristic value is updated by the 
    /// peripheral. The first parameter is the characteristic 
    /// UUID. The second is the raw data bytes that have been 
    /// updated for the characteristic. This method is for 
    /// backwards compatibility. A new method with the device 
    /// address was added in version 2.3 (see below).
    /// </summary>
    /// <param name="name"></param>
    /// <param name="service"></param>
    /// <param name="characteristic"></param>
    /// <param name="notificationAction"></param>
    /// <param name="action"></param>
    public static void SubscribeCharacteristic(string name, string service, string characteristic,
        Action<string> notificationAction, Action<string, byte[]> action)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                name = name.ToUpper();
                service = service.ToUpper();
                characteristic = characteristic.ToUpper();

#if UNITY_IOS || UNITY_TVOS || (EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX))
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] =
 new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [characteristic] =
 notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] =
 new Dictionary<string, Action<string, byte[]>> ();
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] =
                        new Dictionary<string, Action<string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][
                    FullUUID(characteristic).ToLower()] = notificationAction;

                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] =
                        new Dictionary<string, Action<string, byte[]>>();
                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][FullUUID(characteristic).ToLower()] =
                    action;
#endif
            }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidSubscribeCharacteristic", name, service, characteristic);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method will subscribe to a characteristic by 
    /// peripheral name and the service and characteristic. 
    /// The notificationAction callback is called when the 
    /// notification occurs and the action callback is called 
    /// whenever the characteristic value is updated by the 
    /// peripheral. The first parameter is the device address. 
    /// The second parameter is the characteristic UUID. The 
    /// third is the raw data bytes that have been updated for 
    /// the characteristic.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="service"></param>
    /// <param name="characteristic"></param>
    /// <param name="notificationAction"></param>
    /// <param name="action"></param>
    public static void SubscribeCharacteristicWithDeviceAddress(string name, string service, string characteristic,
        Action<string, string> notificationAction, Action<string, string, byte[]> action)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                name = name.ToUpper();
                service = service.ToUpper();
                characteristic = characteristic.ToUpper();

#if UNITY_IOS || UNITY_TVOS || (EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX))
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] =
 new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic]
 = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] =
 new Dictionary<string, Action<string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] =
 new Dictionary<string, Action<string, string, byte[]>>();
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] =
 action;
#elif UNITY_ANDROID
                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction
                        .ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] =
                        new Dictionary<string, Action<string, string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][
                    FullUUID(characteristic).ToLower()] = notificationAction;

                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] =
                        new Dictionary<string, Action<string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][
                    FullUUID(characteristic).ToLower()] = null;

                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] =
                        new Dictionary<string, Action<string, string, byte[]>>();
                bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][
                    FullUUID(characteristic).ToLower()] = action;
#endif
            }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			OSXBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidSubscribeCharacteristic", name, service, characteristic);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    /// <summary>
    /// This method is unsubscribe from a characteristic by 
    /// name, service and characteristic. When complete the 
    /// action callback is called
    /// </summary>
    /// <param name="name"></param>
    /// <param name="service"></param>
    /// <param name="characteristic"></param>
    /// <param name="action"></param>
    public static void UnSubscribeCharacteristic(string name, string service, string characteristic,
        Action<string> action)
    {
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        if (!Application.isEditor)
        {
#endif
            if (bluetoothDeviceScript != null)
            {
                name = name.ToUpper();
                service = service.ToUpper();
                characteristic = characteristic.ToUpper();

#if UNITY_IOS || UNITY_TVOS || (EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX))
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] =
 new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic]
 = null;

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] =
 new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = action;
#elif UNITY_ANDROID
                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction
                        .ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] =
                        new Dictionary<string, Action<string, string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][
                    FullUUID(characteristic).ToLower()] = null;

                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] =
                        new Dictionary<string, Action<string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][
                    FullUUID(characteristic).ToLower()] = action;
#endif
            }

#if EXPERIMENTAL_MACOS_EDITOR && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
        OSXBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
#elif UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidUnsubscribeCharacteristic", name, service, characteristic);
#endif
#if !UNITY_EDITOR_OSX || !EXPERIMENTAL_MACOS_EDITOR
        }
#endif
    }

    public static void PeripheralName(string newName)
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS
			_iOSBluetoothLEPeripheralName (newName);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidPeripheralName", newName);
#endif
        }
    }

    public static void CreateService(string uuid, bool primary, Action<string> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.ServiceAddedAction = action;

#if UNITY_IOS
			_iOSBluetoothLECreateService (uuid, primary);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidCreateService", uuid, primary);
#endif
        }
    }

    public static void RemoveService(string uuid)
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS
			_iOSBluetoothLERemoveService (uuid);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidRemoveService", uuid);
#endif
        }
    }

    public static void RemoveServices()
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS
			_iOSBluetoothLERemoveServices ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidRemoveServices");
#endif
        }
    }

    public static void CreateCharacteristic(string uuid, CBCharacteristicProperties properties,
        CBAttributePermissions permissions, byte[] data, int length, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

#if UNITY_IOS
			_iOSBluetoothLECreateCharacteristic (uuid, (int)properties, (int)permissions, data, length);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidCreateCharacteristic", uuid, (int) properties, (int) permissions, data, length);
#endif
        }
    }

    public static void RemoveCharacteristic(string uuid)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

#if UNITY_IOS
			_iOSBluetoothLERemoveCharacteristic (uuid);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidRemoveCharacteristic", uuid);
#endif
        }
    }

    public static void RemoveCharacteristics()
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS
			_iOSBluetoothLERemoveCharacteristics ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidRemoveCharacteristics");
#endif
        }
    }

    public static void StartAdvertising(Action action, bool isConnectable = true, bool includeName = true,
        int manufacturerId = 0, byte[] manufacturerSpecificData = null)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.StartedAdvertisingAction = action;

#if UNITY_IOS
			_iOSBluetoothLEStartAdvertising ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidStartAdvertising", isConnectable, includeName, manufacturerId,
                    manufacturerSpecificData);
#endif
        }
    }

    public static void StopAdvertising(Action action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.StoppedAdvertisingAction = action;

#if UNITY_IOS
			_iOSBluetoothLEStopAdvertising ();
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidStopAdvertising");
#endif
        }
    }

    public static void UpdateCharacteristicValue(string uuid, byte[] data, int length)
    {
        if (!Application.isEditor)
        {
#if UNITY_IOS
			_iOSBluetoothLEUpdateCharacteristicValue (uuid, data, length);
#elif UNITY_ANDROID
            if (_android != null)
                _android.Call("androidUpdateCharacteristicValue", uuid, data, length);
#endif
        }
    }

    public static string FullUUID(string uuid)
    {
        if (uuid.Length == 4)
            return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
        return uuid;
    }
}