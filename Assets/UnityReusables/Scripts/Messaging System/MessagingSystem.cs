// using System.Collections.Generic;
// using UnityEngine;
//
// public abstract class Message {
//     public readonly string type;
//     protected Message() { type = GetType().Name; }
// }
//
// public delegate bool MessageHandlerDelegate(Message msg);
//
// /// <summary>
// /// Usage :
// /// 1. attach a listener given a type (Derived from Message) and a function to execute when the message is received
// /// - MessagingSystem.Instance.AttachListener(typeof(MyCustomMessage), this.HandleMyCustomMessage);
// /// - bool HandleMyCustomMessage(Message msg) { return true to consume the message, false for other to receive it as well}
// /// 2. remove the listener on destroy or when no longer needed
// /// - MessagingSystem.Instance.DetachListener(typeof(MyCustomMessage), HandleMyCustomMessage);
// ///  
// /// </summary>
// public class MessagingSystem : SingletonComponent<MessagingSystem>
// {
//     public static MessagingSystem Instance
//     {
//         get => (MessagingSystem) _Instance;
//         set => _Instance = value;
//     }
//     
//     private Dictionary<string, List<MessageHandlerDelegate>> _listenerDict = new Dictionary<string, List<MessageHandlerDelegate>>();
//     private Queue<Message> _messageQueue = new Queue<Message>();
//     private const int MaxQueueProcessingTime = 16667;
//     private System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();
//
//     private void Update()
//     {
//         _timer.Reset();
//         _timer.Start();
//         while (_messageQueue.Count > 0)
//         {
//             if (_timer.ElapsedMilliseconds > MaxQueueProcessingTime)
//             {
//                 _timer.Stop();
//                 return;
//             }
//
//             Message msg = _messageQueue.Dequeue();
//             if (!TriggerMessage(msg))
//             {
//                 Debug.LogError($"Error when processing message: {msg.type}");
//             }
//         }
//     }
//
//     /// <summary>
//     /// Use this method to process an event immediately, if it is frame-critical
//     /// 
//     /// </summary>
//     public bool TriggerMessage(Message msg)
//     {
//         string msgType = msg.type;
//         if (!_listenerDict.ContainsKey(msgType))
//         {
//             Debug.Log($"MessagingSystem: Message {msgType} has no listeners!");
//             return false; // no listeners so ignored
//         }
//
//         var listenerList = _listenerDict[msgType];
//         foreach (var listener in listenerList)
//         {
//             // if the delegate returns true, the message is consumed, if not, it is passed to the next listener
//             if (listener(msg)) return true; // message consumed by the delegate
//         }
//         return true;
//     }
//
//     /// <summary>
//     /// Use this method to queue an event that is not frame-critical
//     /// </summary>
//     public bool QueueMessage(Message msg)
//     {
//         if (!_listenerDict.ContainsKey(msg.type)) return false;
//         _messageQueue.Enqueue(msg);
//         return true;
//     }
//     
//     /// <summary>
//     ///   <para>Attach a listener to a specified type, if the listener returns true, it consumes the message and no other listener will process it</para>
//     /// </summary>
//     /// <param name="type">A message type</param>
//     /// <param name="handler">To send the message to when the given message type comes through the system.</param>
//     public bool AttachListener(System.Type type, MessageHandlerDelegate handler)
//     {
//         if (type == null)
//         {
//             Debug.Log("MessagingSystem: AttachListener failed due to having no message type specified");
//             return false;
//         }
//
//         string msgType = type.Name;
//         if (!_listenerDict.ContainsKey(msgType))
//         {
//             _listenerDict.Add(msgType, new List<MessageHandlerDelegate>());
//         }
//
//         var listenerList = _listenerDict[msgType];
//         if (listenerList.Contains(handler))
//         {
//             return false; // listener already in list
//         }
//         
//         listenerList.Add(handler);
//         return true;
//     }
//
//     public bool DetachListener(System.Type type, MessageHandlerDelegate handler)
//     {
//         if (type == null)
//         {
//             Debug.Log("MessagingSystem: DetachListener failed due to having no message type specified");
//             return false;
//         }
//
//         string msgType = type.Name;
//         if (!_listenerDict.ContainsKey(msgType))
//             return false;
//
//         var listenerList = _listenerDict[msgType];
//         if (!listenerList.Contains(handler))
//         {
//             return false;
//         }
//
//         listenerList.Remove(handler);
//         return true;
//     }
// }
