using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace TFTPServerApp
{
    [DataContract]
    public enum TFTPLogState
    {
        [EnumMember]
        Busy,
        [EnumMember]
        Stopped,
        [EnumMember]
        Completed
    }

    [DataContract]
    public class TFTPLogEntry
    {
        [DataMember]
        public string Server;

        [DataMember]
        public long Id;

        [DataMember]
        public DateTime StartTime;

        [DataMember]
        public bool IsUpload;

        [DataMember]
        public string Filename;

        [DataMember]
        public long FileLength;

        [DataMember]
        public IPEndPoint LocalEndPoint;

        [DataMember]
        public IPEndPoint RemoteEndPoint;

        [DataMember]
        public int WindowSize;

        [DataMember]
        public TFTPLogState State;

        [DataMember]
        public long Transferred;

        [DataMember]
        public double Speed;

        [DataMember]
        public string ErrorMessage;
    }

    [ServiceContract]
    interface ITFTPServiceContract
    {
        [OperationContract]
        List<TFTPLogEntry> GetLogEntries();
    }

    public class TFTPServiceContractImpl : ITFTPServiceContract
    {
        private static TFTPLogState Convert(CodePlex.JPMikkers.TFTP.SessionLogEntry.TState state)
        {
            TFTPLogState result;

            switch(state)
            {
                case CodePlex.JPMikkers.TFTP.SessionLogEntry.TState.Busy:
                    result = TFTPLogState.Busy;
                    break;

                case CodePlex.JPMikkers.TFTP.SessionLogEntry.TState.Completed:
                    result = TFTPLogState.Completed;
                    break;

                case CodePlex.JPMikkers.TFTP.SessionLogEntry.TState.Stopped:
                default:
                    result = TFTPLogState.Stopped;
                    break;
            }
            return result;
        }

        public List<TFTPLogEntry> GetLogEntries()
        {
            List<TFTPLogEntry> result = new List<TFTPLogEntry>();

            foreach(var server in TFTPService.Instance.GetServers())
            {
                if(server != null)
                {
                    foreach(var item in server.SessionLog.GetHistory())
                    {
                        result.Add(
                            new TFTPLogEntry()
                            {
                                Id = item.Id,
                                ErrorMessage = item.Exception != null ? item.Exception.Message : "",
                                FileLength = item.Configuration.FileLength,
                                Filename = item.Configuration.Filename,
                                IsUpload = item.Configuration.IsUpload,
                                LocalEndPoint = item.Configuration.LocalEndPoint,
                                RemoteEndPoint = item.Configuration.RemoteEndPoint,
                                Server = server.Name,
                                StartTime = item.Configuration.StartTime,
                                State = Convert(item.State),
                                WindowSize = item.Configuration.WindowSize,
                                Transferred = item.Transferred,
                                Speed = item.Speed
                            });
                    }
                }
            }

            return result;
        }
    }
}
