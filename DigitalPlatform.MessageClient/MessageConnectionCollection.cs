﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// MessageConnection 的集合
    /// </summary>
    public class MessageConnectionCollection : IDisposable
    {
        List<MessageConnection> _connections = new List<MessageConnection>();

        public event LoginEventHandler Login = null;

        // parameters:
        //      strName 连接的名字。如果要针对同一 dp2mserver 使用多根连接，可以用名字区分它们。如果不想区分，可以使用空
        public Task<MessageConnection> GetConnectionAsync(string url,
    string strName,
    bool autoConnect = true)
        {
            MessageConnection connection = null;
            foreach (MessageConnection current_connection in _connections)
            {
                if (current_connection.ServerUrl == url && current_connection.Name == strName)
                {
                    connection = current_connection;
                    goto FOUND;
                }
            }

            connection = new MessageConnection();
            connection.ServerUrl = url;
            connection.Name = strName;
            connection.Container = this;
            this._connections.Add(connection);

        FOUND:
            if (autoConnect && connection.IsConnected == false)
            {
                Task<MessageConnection> task = new Task<MessageConnection>(() =>
                {
                    connection.ConnectAsync(url).Wait();
                    return connection;
                });
                task.Start();
                return task;
            }

            {
                var task = new Task<MessageConnection>(() =>
                {
                    return connection;
                });
                task.Start();
                return task;
            }
        }

#if NO
        // parameters:
        //      strName 连接的名字。如果要针对同一 dp2mserver 使用多根连接，可以用名字区分它们。如果不想区分，可以使用空
        //      autoConnect 是否自动连接
        //      waitConnecting  是否等待连接完成后再返回?
        public MessageConnection GetConnection(string url,
            string remoteName,
            bool autoConnect = true,
            bool waitConnecting = true)
        {
            MessageConnection connection = null;
            foreach (MessageConnection current_connection in _connections)
            {
                if (current_connection.ServerUrl == url && current_connection.Name == remoteName)
                {
                    connection = current_connection;
                    goto FOUND;
                }
            }

            connection = new MessageConnection();
            connection.ServerUrl = url;
            connection.Name = remoteName;
            connection.Container = this;
            this._connections.Add(connection);

        FOUND:
            if (autoConnect && connection.IsConnected == false)
            {
                Task task = connection.ConnectAsync(url);

                if (waitConnecting)
                    task.Wait();
            }

            return connection;
        }
#endif

        public void DeleteConnection(MessageConnection channel)
        {
            this._connections.Remove(channel);
        }

        public void Clear()
        {
            foreach (MessageConnection channel in this._connections)
            {
                channel.CloseConnection();
            }

            this._connections.Clear();
        }

        public void Dispose()
        {
            this.Clear();
        }

        // 触发登录事件
        public virtual void TriggerLogin(MessageConnection connection)
        {
            if (this.Login != null)
            {
                LoginEventArgs e = new LoginEventArgs();
                this.Login(connection, e);
            }
        }
    }

    /// <summary>
    /// 登录事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void LoginEventHandler(object sender,
    LoginEventArgs e);

    /// <summary>
    /// 登录事件的参数
    /// </summary>
    public class LoginEventArgs : EventArgs
    {
        // public string ErrorInfo = "";   // [out] 出错信息
    }
}
