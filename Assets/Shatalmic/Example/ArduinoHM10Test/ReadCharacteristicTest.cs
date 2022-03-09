/* This is an example to show how to connect to 2 HM-10 devices
 * that are connected together via their serial pins and send data
 * back and forth between them.
 */

using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Shatalmic.Example.ArduinoHM10Test
{
    public class ReadCharacteristicTest : MonoBehaviour
    {
        enum States
        {
            None,
            Scan,
            Connect,
            RequestMtu,
            Subscribe,
            Unsubscribe,
            Disconnect,
            Communication,
        }

        #region Fields

        [FormerlySerializedAs("Status")] [Header("Set Up")] [FormerlySerializedAs("HM10_Status")] [SerializeField]
        private Text status;

        [SerializeField] private ScrollRect statusScrollRect;

        [FormerlySerializedAs("BluetoothStatus")] [SerializeField]
        private Text bluetoothStatus;

        [FormerlySerializedAs("PanelMiddle")] [SerializeField]
        private GameObject panelMiddle;

        [FormerlySerializedAs("DeviceNameInput")] [SerializeField]
        private InputField deviceNameInput;

        [FormerlySerializedAs("ServiceUuidInput")] [SerializeField]
        private InputField serviceUuidInput;

        [FormerlySerializedAs("CharacteristicUuidInput")] [SerializeField]
        private InputField characteristicUuidInput;

        private bool _connected = false;
        private float _timeout = 0f;
        private States _state = States.None;
        private bool _foundID = false;

        private string deviceAddress;
        private string deviceName;

        #endregion

        #region Properties

        private string DeviceNameText => deviceNameInput.text;
        private string ServiceUuidText => serviceUuidInput.text;
        private string CharacteristicUuidText => characteristicUuidInput.text;

        public string Status
        {
            get => status.text;
            set
            {
                status.text += '\n' + value;

                // Scroll down after added status message
                statusScrollRect.normalizedPosition = new Vector2();
            }
        }

        #endregion

        #region Monobehaviour

        private void Awake()
        {
            Assert.IsNotNull(status);
            Assert.IsNotNull(bluetoothStatus);
            Assert.IsNotNull(panelMiddle);
            Assert.IsNotNull(deviceNameInput);
            Assert.IsNotNull(serviceUuidInput);
            Assert.IsNotNull(characteristicUuidInput);
            Assert.IsNotNull(statusScrollRect);
        }

        private void Start()
        {
            Initialise();
        }

        void Update()
        {
            if (_timeout > 0f)
            {
                _timeout -= Time.deltaTime;
                if (_timeout <= 0f)
                {
                    _timeout = 0f;

                    switch (_state)
                    {
                        case States.None:
                            break;

                        case States.Scan:
                            bluetoothStatus.text = $"Scanning for {DeviceNameText} devices...";
                            Debug.Log($"Scanning for {DeviceNameText} devices...");
                            Status = $"Scanning for {DeviceNameText} devices...";

                            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                                null, (address, name) =>
                                {
                                    // we only want to look at devices that have the name we are looking for
                                    // this is the best way to filter out devices
                                    if (name.ToLower().Contains(DeviceNameText.ToLower()))
                                    {
                                        // it is always a good idea to stop scanning while you connect to a device
                                        // and get things set up
                                        BluetoothLEHardwareInterface.StopScan();
                                        bluetoothStatus.text = "";

                                        // add it to the list and set to connect to it
                                        deviceAddress = address;
                                        deviceName = name;

                                        Status = $"Found {deviceName}";
                                        Debug.Log($"Found {deviceName}");

                                        SetState(States.Connect, 0.5f);
                                    }
                                }, null, false, false);
                            break;

                        case States.Connect:
                            // set these flags
                            _foundID = false;

                            Status = $"Connecting to {deviceName}";
                            Debug.Log($"Connecting to {deviceName}");

                            // note that the first parameter is the address, not the name. I have not fixed this because
                            // of backwards compatibility.
                            // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                            // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                            // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                            BluetoothLEHardwareInterface.ConnectToPeripheral(deviceAddress, address =>
                                {
                                    Status = $"Connected to {deviceName}, {address}...";
                                    Debug.Log($"Connected to {deviceName}, {address}...");
                                }, null,
                                (address, serviceUuid, characteristicUuid) =>
                                {
                                    // Commented because i use the button to read characteristics

                                    // if we have found the characteristic that we are waiting for
                                    // set the state. make sure there is enough timeout that if the
                                    // device is still enumerating other characteristics it finishes
                                    // before we try to subscribe
                                    // if (IsEqual(serviceUuid, ServiceUUID) &&
                                    //     IsEqual(characteristicUuid, Characteristic))
                                    // {
                                    //     _connected = true;
                                    //
                                    //     SetState(States.RequestMtu, 2f);
                                    // }
                                }, (disconnectedAddress) =>
                                {
                                    BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                                    Status = $"Disconnected {disconnectedAddress}";
                                    Debug.Log($"Disconnected {disconnectedAddress}");
                                });
                            break;

                        case States.RequestMtu:
                            Status = "Requesting MTU";
                            Debug.Log("Requesting MTU");

                            BluetoothLEHardwareInterface.RequestMtu(deviceAddress, 185, (address, newMTU) =>
                            {
                                Status = "MTU set to " + newMTU;
                                Debug.Log("MTU set to " + newMTU);

                                SetState(States.Subscribe, 0.1f);
                            });
                            break;

                        case States.Subscribe:
                            Status = $"Subscribing to {DeviceNameText}";
                            Debug.Log($"Subscribing to {DeviceNameText}");

                            BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(deviceAddress,
                                ServiceUuidText, CharacteristicUuidText, null,
                                (address, characteristicUuid, bytes) =>
                                {
                                    Status = "Received data: " + Encoding.UTF8.GetString(bytes);
                                    Debug.Log("Received data: " + Encoding.UTF8.GetString(bytes));
                                });

                            // set to the none state and the user can start sending and receiving data
                            _state = States.None;
                            Status = "Waiting...";
                            Debug.Log("Waiting...");

                            break;

                        case States.Unsubscribe:
                            BluetoothLEHardwareInterface.UnSubscribeCharacteristic(deviceAddress, ServiceUuidText,
                                CharacteristicUuidText, null);
                            SetState(States.Disconnect, 4f);
                            break;

                        case States.Disconnect:
                            if (_connected)
                            {
                                BluetoothLEHardwareInterface.DisconnectPeripheral(deviceAddress, (address) =>
                                {
                                    BluetoothLEHardwareInterface.DeInitialize(() =>
                                    {
                                        _connected = false;
                                        _state = States.None;
                                    });
                                });
                            }
                            else
                            {
                                BluetoothLEHardwareInterface.DeInitialize(() => { _state = States.None; });
                            }

                            break;
                    }
                }
            }
        }

        void Reset()
        {
            _connected = false;
            _timeout = 0f;
            _state = States.None;
            _foundID = false;
            deviceAddress = null;
        }

        #endregion


        #region Methods

        public void Initialise()
        {
            Reset();
            bluetoothStatus.text = "Initializing...";
            Debug.Log("Initializing...");
            Status = "Initializing...";

            BluetoothLEHardwareInterface.Initialize(BluetoothDeviceRole.Central, () =>
            {
                bluetoothStatus.text = "Initialized";
                Debug.Log("Initialized");
                Status = "Initialized";
            }, error =>
            {
                bluetoothStatus.text = "Error: " + error;
                Debug.Log("Error: " + error);
                Status = $"Error: {error}";
            });
        }

        public void Connect()
        {
            SetState(States.Scan, .5f);
        }

        public void ReadCharacteristic()
        {
            Status = "Read characteristic...";
            BluetoothLEHardwareInterface.ReadCharacteristic(deviceName,
                FullUuid(ServiceUuidText),
                FullUuid(CharacteristicUuidText),
                (characteristicUuid, bytes) =>
                {
                    Status = $"Received data {Encoding.UTF8.GetString(bytes)}";
                });
        }

        public void SubscribeCharacteristic()
        {
            Status = "Subscribe characteristic...";
            SetState(States.Subscribe, 0.1f);
        }

        public void ListConnectedDevices()
        {
            BluetoothLEHardwareInterface.StopScan();

            Status = "List devices:";
            BluetoothLEHardwareInterface.RetrieveListOfPeripheralsWithServices(null,
                ((address, name) => { Status = $"ConnDev: {name}, {address}"; }));
        }

        public void RequestMtu()
        {
            SetState(States.RequestMtu, 2f);
        }

        void SetState(States newState, float timeout)
        {
            _state = newState;
            _timeout = timeout;
        }


        string FullUuid(string uuid)
        {
            return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
        }

        bool IsEqual(string uuid1, string uuid2)
        {
            if (uuid1.Length == 4)
                uuid1 = FullUuid(uuid1);
            if (uuid2.Length == 4)
                uuid2 = FullUuid(uuid2);

            return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
        }

        void SendString(string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            // notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
            // some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
            // the device
            BluetoothLEHardwareInterface.WriteCharacteristic(deviceAddress, ServiceUuidText, CharacteristicUuidText,
                data,
                data.Length,
                false, (characteristicUUID) => { BluetoothLEHardwareInterface.Log("Write Succeeded"); });
        }

        void SendByte(byte value)
        {
            byte[] data = new byte[] {value};
            // notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
            // some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
            // the device
            BluetoothLEHardwareInterface.WriteCharacteristic(deviceAddress, ServiceUuidText, CharacteristicUuidText,
                data,
                data.Length,
                false, 
                (characteristicUUID) => { BluetoothLEHardwareInterface.Log("Write Succeeded"); });
        }

        #endregion
    }
}