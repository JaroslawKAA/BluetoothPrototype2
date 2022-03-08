using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Level1Script : MonoBehaviour
{
	public void OnScanClick ()
	{
		BluetoothLEHardwareInterface.Initialize (BluetoothDeviceRole.Central, () => {

			FoundDeviceListScript.DeviceAddressList = new List<DeviceObject> ();

			BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {

				FoundDeviceListScript.DeviceAddressList.Add (new DeviceObject (address, name));

			}, null);

		}, (error) => {

			BluetoothLEHardwareInterface.Log ("BLE Error: " + error);

		});
	}

	public void OnStartLevel2 ()
	{
		SceneManager.LoadScene ("Level2");
	}
}
