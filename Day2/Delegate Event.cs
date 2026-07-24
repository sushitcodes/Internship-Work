// AlarmClock.cs — a class that raises an event
    // Delegate: defines the "shape" of a method that can be plugged in
    public delegate void AlarmHandler(string message);

    class AlarmClock
    {
        // Event: other classes can subscribe to this, but only AlarmClock can trigger it
        public event AlarmHandler OnAlarmRing;

        public void RingAlarm()
        {
            Console.WriteLine("Alarm is ringing!");
            OnAlarmRing?.Invoke("Wake up! It's time!"); // ?. avoids error if nobody subscribed
        }
    }   