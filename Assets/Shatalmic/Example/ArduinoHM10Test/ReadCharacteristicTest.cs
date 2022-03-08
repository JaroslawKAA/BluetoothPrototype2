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
            RequestMTU,
            Subscribe,
            Unsubscribe,
            Disconnect,
            Communication,
        }

        #region Fields

        [Header("Set Up")] [FormerlySerializedAs("HM10_Status")] [SerializeField]
        private Text Status;

        [SerializeField] private Text BluetoothStatus;
        [SerializeField] private GameObject PanelMiddle;
        [SerializeField] private InputField DeviceNameInput;
        [SerializeField] private InputField ServiceUuidInput;
        [SerializeField] private InputField CharacteristicUuidInput;

        private bool _workingFoundDevice = true;
        private bool _connected = false;
        private float _timeout = 0f;
        private States _state = States.None;
        private bool _foundID = false;
        private string deviceAddress;

        #endregion

        #region Properties

        private string DeviceName => DeviceNameInput.text;
        private string ServiceUUID => ServiceUuidInput.text;
        private string Characteristic => CharacteristicUuidInput.text;

        #endregion

        #region Monobehaviour

        private void Awake()
        {
            Assert.IsNotNull(Status);
            Assert.IsNotNull(BluetoothStatus);
            Assert.IsNotNull(PanelMiddle);
            Assert.IsNotNull(DeviceNameInput);
            Assert.IsNotNull(ServiceUuidInput);
            Assert.IsNotNull(CharacteristicUuidInput);
        }

        void Start()
        {
            Status.text = "";
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
                            BluetoothStatus.text = "Scanning for Mouse Arc devices...";

                            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                                null, (address, name) =>
                                {
                                    // we only want to look at devices that have the name we are looking for
                                    // this is the best way to filter out devices
                                    if (name.ToLower().Contains(DeviceName.ToLower()))
                                    {
                                        _workingFoundDevice = true;

                                        // it is always a good idea to stop scanning while you connect to a device
                                        // and get things set up
                                        BluetoothLEHardwareInterface.StopScan();
                                        BluetoothStatus.text = "";

                                        // add it to the list and set to connect to it
                                        deviceAddress = address;

                                        Status.text = "Found Arc Mouse";

                                        SetState(States.Connect, 0.5f);

                                        _workingFoundDevice = false;
                                    }
                                }, null, false, false);
                            break;

                        case States.Connect:
                            // set these flags
                            _foundID = false;

                            Status.text = $"Connecting to {DeviceName}";

                            // note that the first parameter is the address, not the name. I have not fixed this because
                            // of backwards compatiblity.
                            // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                            // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                            // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                            BluetoothLEHardwareInterface.ConnectToPeripheral(deviceAddress, null, null,
                                (address, serviceUUID, characteristicUUID) =>
                                {
                                    if (IsEqual(serviceUUID, ServiceUUID))
                                    {
                                        // if we have found the characteristic that we are waiting for
                                        // set the state. make sure there is enough timeout that if the
                                        // device is still enumerating other characteristics it finishes
                                        // before we try to subscribe
                                        if (IsEqual(characteristicUUID, Characteristic))
                                        {
                                            _connected = true;

                                            SetState(States.RequestMTU, 2f);

                                            Status.text = $"Connected to {DeviceName}...";
                                        }
                                    }
                                }, (disconnectedAddress) =>
                                {
                                    BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                                    Status.text = "Disconnected";
                                });
                            break;

                        case States.RequestMTU:
                            Status.text = "Requesting MTU";

                            BluetoothLEHardwareInterface.RequestMtu(deviceAddress, 185, (address, newMTU) =>
                            {
                                Status.text = "MTU set to " + newMTU.ToString();

                                SetState(States.Subscribe, 0.1f);
                            });
                            break;

                        case States.Subscribe:
                            Status.text = $"Subscribing to {DeviceName}";

                            BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(deviceAddress,
                                ServiceUUID, Characteristic, null,
                                (address, characteristicUUID, bytes) =>
                                {
                                    Status.text = "Received Serial: " + Encoding.UTF8.GetString(bytes);
                                });

                            // set to the none state and the user can start sending and receiving data
                            _state = States.None;
                            Status.text = "Waiting...";

                            PanelMiddle.SetActive(true);
                            break;

                        case States.Unsubscribe:
                            BluetoothLEHardwareInterface.UnSubscribeCharacteristic(deviceAddress, ServiceUUID,
                                Characteristic, null);
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
            _workingFoundDevice =
                false; // used to guard against trying to connect to a second device while still connecting to the first
            _connected = false;
            _timeout = 0f;
            _state = States.None;
            _foundID = false;
            deviceAddress = null;
        }

        #endregion


        #region Methods

        public void StartProcess()
        {
            Reset();
            BluetoothStatus.text = "Initializing...";

            BluetoothLEHardwareInterface.Initialize(BluetoothDeviceRole.Central, () =>
            {
                SetState(States.Scan, 0.1f);

                BluetoothStatus.text = "Initialized";
            }, error => BluetoothStatus.text = "Error: " + error);
        }

        void SetState(States newState, float timeout)
        {
            _state = newState;
            _timeout = timeout;
        }


        string FullUUID(string uuid)
        {
            return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
        }

        bool IsEqual(string uuid1, string uuid2)
        {
            if (uuid1.Length == 4)
                uuid1 = FullUUID(uuid1);
            if (uuid2.Length == 4)
                uuid2 = FullUUID(uuid2);

            return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
        }

        void SendString(string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            // notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
            // some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
            // the device
            BluetoothLEHardwareInterface.WriteCharacteristic(deviceAddress, ServiceUUID, Characteristic, data,
                data.Length,
                false, (characteristicUUID) => { BluetoothLEHardwareInterface.Log("Write Succeeded"); });
        }

        void SendByte(byte value)
        {
            byte[] data = new byte[] {value};
            // notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
            // some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
            // the device
            BluetoothLEHardwareInterface.WriteCharacteristic(deviceAddress, ServiceUUID, Characteristic, data,
                data.Length,
                false, (characteristicUUID) => { BluetoothLEHardwareInterface.Log("Write Succeeded"); });
        }

        #endregion
    }
}