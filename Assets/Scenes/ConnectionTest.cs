using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Scenes
{
    public class ConnectionTest : MonoBehaviour
    {
        #region Fields

        private string MacAddress = String.Empty;
        private string ServiceUUID = String.Empty;
        private string CharacteristicUUID = String.Empty;
        
        [SerializeField] private Text statusText;
        [SerializeField] private DetectedDeviceEntity deviceEntityPrefab;
        [SerializeField] private Transform deviceEntitiesContainer;
        [SerializeField] private Button scanAgainButton;
        [SerializeField] private ScrollRect statusTextScrollRect;

        private List<string> detectedDevices = new List<string>();

        #endregion

        #region Properties

        public static ConnectionTest Instance { get; private set; }


        private string StatusMessage
        {
            get => statusText.text;
            set
            {
                statusText.text = value + '\n';
                statusTextScrollRect.normalizedPosition = new Vector2();
            }
        }

        #endregion

        #region Monobehaviour

        private void Awake()
        {
            Assert.IsNotNull(statusText);
            Assert.IsNotNull(deviceEntityPrefab);
            Assert.IsNotNull(deviceEntitiesContainer);
            Assert.IsNotNull(scanAgainButton);
            Assert.IsNotNull(statusTextScrollRect);

            Instance = this;

            scanAgainButton.interactable = false;
            scanAgainButton.onClick.AddListener(OnScanAgain);
        }

        private void Start()
        {
            BluetoothLEHardwareInterface.Initialize(BluetoothDeviceRole.Central, ScanDevices,
                error => { StatusMessage += "Error during initialize: " + error; });
        }

        private void ScanDevices()
        {
            scanAgainButton.interactable = false;
            
            StatusMessage += "Scanning...";

            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) =>
            {
                if (!detectedDevices.Contains(address))
                {
                    DetectedDeviceEntity deviceEntity = Instantiate(deviceEntityPrefab, deviceEntitiesContainer);
                    deviceEntity.Init(name, address);
                    detectedDevices.Add(address);
                }
            });
        }

        private void OnDestroy()
        {
            scanAgainButton.onClick.RemoveListener(OnScanAgain);
        }

        #endregion

        public void Connect(string macAddress)
        {
            BluetoothLEHardwareInterface.StopScan();
            StatusMessage += "Stop scan...";
            scanAgainButton.interactable = true;
            
            BluetoothLEHardwareInterface.DisconnectAll();
            StatusMessage += "Disconnect all...";

            StatusMessage += "Connecting...";
            BluetoothLEHardwareInterface.ConnectToPeripheral(macAddress, name =>
                {
                    StatusMessage += $"\nConnected with: {name} (Connect action invoked)";
                },
                (address, serviceUuid) =>
                {
                    StatusMessage += $"Connected... (Service action invoked), {address}, {serviceUuid}";
                },
                (address, serviceUUID, characteristicUUID) =>
                {
                    StatusMessage += "\nConnected... (Characteristic action invoked)";

                    this.ServiceUUID = FullUUID(serviceUUID);
                    this.CharacteristicUUID = FullUUID(characteristicUUID);
                    this.MacAddress = address;
                    
                    StatusMessage += $"Service: {ServiceUUID}";
                    StatusMessage += $"Characteristic: {CharacteristicUUID}";
                    StatusMessage += $"Mac: {MacAddress}";
                    
                    StartCoroutine(Subscribe());
                }, disconnectedAddress =>
                {
                    StatusMessage += $"Device Disconnected: {disconnectedAddress}";
                });
        }

        public void OnScanAgain()
        {
            BluetoothLEHardwareInterface.StopScan();

            StatusMessage += "Scan again...";
            
            detectedDevices.Clear();

            foreach (Transform entity in deviceEntitiesContainer)
                Destroy(entity.gameObject);
            
            ScanDevices();
        }

        private IEnumerator RequestMtu(string macAddress)
        {
            yield return new WaitForSeconds(1f);

            StatusMessage += "Requesting MTU";

            BluetoothLEHardwareInterface.RequestMtu(macAddress, 185, (address, newMtu) =>
            {
                StatusMessage += "MTU set to " + newMtu;

                StartCoroutine(Subscribe());

                StatusMessage += "Waiting...";
            });
        }

        private IEnumerator Subscribe()
        {
            yield return new WaitForSeconds(.1f);

            StatusMessage += $"Subscribing to: {ServiceUUID}, {CharacteristicUUID}";
            
            BluetoothLEHardwareInterface.SubscribeCharacteristic(MacAddress, ServiceUUID,
                CharacteristicUUID, null,
                (name, bytes) =>
                {
                    StatusMessage += $"Received data: {name}, {Encoding.UTF8.GetString(bytes)}";
                });
        }

        string FullUUID(string uuid)
        {
            if (uuid.Length == 4)
                return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";

            return uuid;
        }
    }
}