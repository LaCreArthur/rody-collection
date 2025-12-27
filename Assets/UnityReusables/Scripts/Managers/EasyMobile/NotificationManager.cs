#if EASY_MOBILE
using System;
using EasyMobile;
using UnityReusables.Utils.Extensions;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Managers/Notification")]
public class NotificationManager : ScriptableObject
{

    [Header("Repeated Local Notification")]
    public int hours = 24;
    public string[] titles;
    public string[] subtitles;
    public string[] bodies;

#if EASY_MOBILE
    void OnEnable()
    {
        Debug.Log("test log Review Manager");
        // Cancels all pending local notifications.
        Notifications.CancelAllPendingLocalNotifications();
        // schedule a new one for the next 24h
        ScheduleRepeatLocalNotification();
    }

    
    NotificationContent PrepareNotificationContent()
    {
        NotificationContent content = new NotificationContent();
        // Provide the notification title.
        content.title = titles.GetRandom();

        // You can optionally provide the notification subtitle, which is visible on iOS only.
        content.subtitle = subtitles.GetRandom();

        // Provide the notification message.
        content.body = bodies.GetRandom();
        
        return content;
    }

    
    void ScheduleRepeatLocalNotification()
    {

        // Prepare the notification content (see the above section).
        NotificationContent content = PrepareNotificationContent();

        // Set the delay time as a TimeSpan.
        TimeSpan delay = new TimeSpan(hours, 00, 00);

        // Schedule the notification.
        Notifications.ScheduleLocalNotification(delay, content, NotificationRepeat.EveryDay);
    }
#endif
    
    /*
     
    // Subscribes to notification events.
    void OnEnable()
    {
        Notifications.LocalNotificationOpened += OnLocalNotificationOpened;
    }

    // Unsubscribes notification events.
    void OnDisable()
    {
        Notifications.LocalNotificationOpened -= OnLocalNotificationOpened;
    }

    // This handler will be called when a local notification is opened.
    void OnLocalNotificationOpened(LocalNotification delivered)
    {
        // The actionId will be empty if the notification was opened with the default action.
        // Otherwise it contains the ID of the selected action button.
        if (!string.IsNullOrEmpty(delivered.actionId))
        {
            Debug.Log("Action ID: " + delivered.actionId);
        }

        // Whether the notification is delivered when the app is in foreground.
        Debug.Log("Is app in foreground: " + delivered.isAppInForeground.ToString());

        // Gets the notification content.
        NotificationContent content = delivered.content;

        // Take further actions if needed...
    }
    
    */
}
