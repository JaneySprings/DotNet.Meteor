namespace DotNet.Mobile.Debug.Entities;

public class Thread {
    public int id { get; }
    public string name { get; }

    public Thread(int id, string name) {
        this.id = id;
        this.name = string.IsNullOrEmpty(name) ? string.Format("Thread #{0}", id) : name;
    }
}