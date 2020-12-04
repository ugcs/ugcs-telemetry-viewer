using Avalonia.Collections;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;
using AppContext = UGCS.TelemetryViewer.Helpers.AppContext;

namespace UGCS.TelemetryViewer.Services
{
    public class StorageService : IStorageService
    {

        [DataContract]
        public class TelemetryPlateData
        {
            [DataMember]
            public string PlateName { get; set; }
            [DataMember]
            public double? MinThreshold { get; set; }
            [DataMember]
            public double? MaxThreshold { get; set; }
            [DataMember]
            public string Units { get; set; }
            [DataMember]
            public int DecimalPlaces { get; set; }
            [DataMember]
            public string TelemetryCodeKey { get; set; }
        }

        [DataContract]
        private class SerializableAppContext
        {
            [DataMember]
            public Dictionary<int, List<TelemetryPlateData>> PlateData { get; set; }
            [DataMember]
            public double MainWindowWidth { get; set; }
            [DataMember]
            public double MainWindowHeight { get; set; }
            [DataMember]
            public int MainWindowPosX { get; set; }
            [DataMember]
            public int MainWindowPosY { get; set; }
        }

        public class Options
        {
            public string Path { get; set; }
        }

        private readonly string _path;
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(StorageService));
        private readonly ITelemetryPlateFactory _telemetryPlateFactory;

        public StorageService(IOptions<Options> options, ITelemetryPlateFactory telemetryPlateFactory)
        {
            if (options == null || options.Value == null)
                throw new ArgumentNullException(nameof(options));
            _telemetryPlateFactory = telemetryPlateFactory ?? throw new ArgumentNullException(nameof(telemetryPlateFactory));
            _path = options.Value.Path + "/appContext.json";
        }

        public bool TryLoadAppContext(out AppContext appContext)
        {
            if (!File.Exists(_path))
            {
                appContext = null;
                return false;
            }
            FileStream stream = null;
            try
            {
                stream = File.Open(_path, FileMode.OpenOrCreate, FileAccess.Read);
                DataContractJsonSerializer jsonSerializer =
                    new DataContractJsonSerializer(typeof(SerializableAppContext));
                SerializableAppContext loadedValue = (SerializableAppContext)jsonSerializer.ReadObject(stream);

                var context = new AppContext
                {
                    vehiclePlatesMap = new Dictionary<int, AvaloniaList<ITelemetryPlate>>()
                };

                if (loadedValue.PlateData != null)
                {
                    foreach (var kvp in loadedValue.PlateData)
                    {
                        AvaloniaList<ITelemetryPlate> list = new AvaloniaList<ITelemetryPlate>();
                        foreach (var plateData in kvp.Value)
                        {
                            ITelemetryPlate plate = _telemetryPlateFactory.Create(
                                plateData.PlateName,
                                plateData.TelemetryCodeKey,
                                plateData.Units,
                                plateData.MinThreshold,
                                plateData.MaxThreshold,
                                plateData.DecimalPlaces);
                            list.Add(plate);
                        }
                        context.vehiclePlatesMap.Add(kvp.Key, list);
                    }
                }

                context.mainWindowWidth = loadedValue.MainWindowWidth;
                context.mainWindowHeight = loadedValue.MainWindowHeight;

                context.mainWindowPosition = new Avalonia.PixelPoint(loadedValue.MainWindowPosX, loadedValue.MainWindowPosY);

                appContext = context;
            }
            catch (Exception err)
            {
                _log.Error("Error occured during loading the app context.", err);
                appContext = null;
                return false;
            }
            finally
            {
                stream.Dispose();
            }
            return true;
        }

        public void StoreAppContext(AppContext appContext)
        {
            if (appContext == null)
                throw new ArgumentNullException(nameof(appContext));
            if (appContext.vehiclePlatesMap == null)
                throw new ArgumentException("Data about plates can not be null");

            Dictionary<int, List<TelemetryPlateData>> serializableDict = new Dictionary<int, List<TelemetryPlateData>>();
            foreach (var kvp in appContext.vehiclePlatesMap)
            {
                List<TelemetryPlateData> serializableList = new List<TelemetryPlateData>();
                foreach (var plate in kvp.Value)
                {
                    TelemetryPlateData data = new TelemetryPlateData()
                    {
                        MaxThreshold = plate.MaxThreshold,
                        MinThreshold = plate.MinThreshold,
                        PlateName = plate.PlateName,
                        Units = plate.Units,
                        TelemetryCodeKey = plate.TelemetryKeyCode,
                        DecimalPlaces = plate.DecimalPlaces,
                    };
                    serializableList.Add(data);
                }
                serializableDict.Add(kvp.Key, serializableList);
            }

            SerializableAppContext serializableContext = new SerializableAppContext
            {
                PlateData = serializableDict,
                MainWindowHeight = appContext.mainWindowHeight,
                MainWindowWidth = appContext.mainWindowWidth,
                MainWindowPosX = appContext.mainWindowPosition.X,
                MainWindowPosY = appContext.mainWindowPosition.Y
            };

            using FileStream stream = File.Create(_path);
            DataContractJsonSerializer jsonSerializer =
                new DataContractJsonSerializer(typeof(SerializableAppContext));
            jsonSerializer.WriteObject(stream, serializableContext);
        }
    }
}
