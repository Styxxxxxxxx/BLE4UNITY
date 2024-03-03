using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using QFramework;

public  class BleDemo1 : MonoBehaviour
    {

    #region 私有字段

    private string uartDeviceName = "UART Service";
    // private string uartDeviceUUID = "BluetoothLE#BluetoothLEe0:4f:43:71:64:ad-dc:da:0c:dd:8c:32";
    // private string uartServiceUUID = "{6e400001-b5a3-f393-e0a9-e50e24dcca9e}";
    // private string uart8ReadCharacteristicUUID = "{6e400003-b5a3-f393-e0a9-e50e24dcca9e}";

    private string uartDeviceUUID;
    private string uartServiceUUID;
    private string uartReadCharacteristicUUID;
    
    private bool isScanningDevices = false;
    private bool isScanningServices = false;
    private bool isScanningCharacteristics = false;
    private bool isSubscribedToCharacteristic = false;
    
    private string foundDeviceId;
    private List<string> foundServiceIds = new ();
    private List<string> foundCharacteristicIds = new List<string>();
    
    #endregion

    private void Start()
    {
        StartDeviceScan();
    }

    void Update()
    {
        #region 设备扫描

        if (isScanningDevices)
        {
            BleApi.DeviceUpdate deviceUpdate = new BleApi.DeviceUpdate();
            BleApi.ScanStatus status = BleApi.PollDevice(ref deviceUpdate, false);
            switch (status)
            {
                case BleApi.ScanStatus.AVAILABLE:
                    
                    if (deviceUpdate.name == uartDeviceName)
                    {
                        Debug.Log("UART SERVICE FOUNF SUCCESSFULLY!");
                        Debug.Log("DEVICE UUID IS:"+deviceUpdate.id);
                        
                        foundDeviceId = deviceUpdate.id;
                        StopDeviceScan();
                        isScanningDevices = false;
                        StartScanServices(foundDeviceId);
                    }
                    break;
                
                case BleApi.ScanStatus.FINISHED:
                    Debug.Log("DEVICE SCAN FINISHED!");
                    isScanningDevices = false;
                    break;
            }
        }

        #endregion
        
        #region 服务扫描

        if (isScanningServices)
        {
            BleApi.Service service = new BleApi.Service();
            BleApi.ScanStatus status = BleApi.PollService(out service, false);

            switch (status)
            {
                case BleApi.ScanStatus.AVAILABLE:
                    if (!foundServiceIds.Contains(service.uuid))
                    {
                        foundServiceIds.Add(service.uuid);
                    }
                    break;
                
                case BleApi.ScanStatus.FINISHED:
                    Debug.Log("SERVICE SCAN FINISHED!");

                    foreach (var VARIABLE in foundServiceIds)
                    {
                        Debug.Log("SERVICES UUID IS: " + VARIABLE);
                    }
                    isScanningServices = false;
                    StartScanCharacteristics(foundDeviceId,foundServiceIds[2]);
                    break;
            }
        }

        #endregion

        #region 特征扫描

        if (isScanningCharacteristics)
        {
            BleApi.Characteristic characteristic = new BleApi.Characteristic();
            BleApi.ScanStatus status = BleApi.PollCharacteristic(out characteristic, false);

            switch (status)
            {
                case BleApi.ScanStatus.AVAILABLE:
                    foundCharacteristicIds.Add(characteristic.uuid);
                    break;

                case BleApi.ScanStatus.FINISHED:
                    Debug.Log("CHARACTERISTICS SCAN FINISHED!");
                    isScanningCharacteristics = false;
                    foreach (var VARIABLE in foundCharacteristicIds)
                    {
                        Debug.Log("CHARACTERISTICS UUID IS:" + VARIABLE);
                    }
                    uartReadCharacteristicUUID = foundCharacteristicIds[0];
                    StartPollData();
                    break;
            }
        }

        #endregion

        #region 订阅特征
        
       
        if (isSubscribedToCharacteristic)
        {
            BleApi.BLEData Data = new BleApi.BLEData();
            BleApi.PollData(out Data, false);
            
            // 将字节数组转换为UTF-8字符串
            string utf8String = Encoding.UTF8.GetString(Data.buf, 0, Data.size);
            // 打印UTF-8字符串
            if (!string.IsNullOrEmpty(utf8String))
            {
                Debug.Log("UTF-8 Data: " + utf8String);
            }
        }

        #endregion
    }
    
    /// <summary>
    /// 开始扫描设备
    /// </summary>
    private void StartDeviceScan()
    {
        isScanningDevices = true;
        BleApi.StartDeviceScan();
    }
    
    /// <summary>
    /// 停止扫描设备
    /// </summary>
    private void StopDeviceScan()
    {
        BleApi.StopDeviceScan();
    }
    
    /// <summary>
    /// 开始扫描服务
    /// </summary>
    /// <param name="deviceId"></param>
    private void StartScanServices(string deviceId)
    {
        isScanningServices = true;
        BleApi.ScanServices(deviceId);
    }  
    
    /// <summary>
    /// 开始扫描特征
    /// </summary>
    /// <param name="deviceId">设备UUID</param>
    /// <param name="serviceId">服务UUID</param>
    private void StartScanCharacteristics(string deviceId, string serviceId)
    {
        isScanningCharacteristics = true;
        BleApi.ScanCharacteristics(deviceId, serviceId);
    }
    
    /// <summary>
    /// 订阅特征后开始轮询数据
    /// </summary>
    private void StartPollData()
    {
        isSubscribedToCharacteristic = true;
        SubscribeToCharacteristic(foundDeviceId,foundServiceIds[2],foundCharacteristicIds[0]);
    }

    /// <summary>
    /// 订阅特征
    /// </summary>
    /// <param name="deviceId">设备UUID</param>
    /// <param name="serviceId">服务UUID</param>
    /// <param name="characteristicId">特征UUID</param>
    private void SubscribeToCharacteristic(string deviceId, string serviceId, string characteristicId)
    {
        BleApi.SubscribeCharacteristic(deviceId, serviceId, characteristicId, false);
        
        Debug.Log($"Subscribed to characteristic {characteristicId} successfully.");
    }
    
    /// <summary>
    /// 在应用程序退出时解除蓝牙连接
    /// </summary>
    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }
}