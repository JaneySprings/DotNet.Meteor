namespace DotNet.Mobile.Debug.Session;

public class Message {
    public int id { get; }
    public string format { get; }
    public dynamic variables { get; }
    public dynamic showUser { get; }
    public dynamic sendTelemetry { get; }

    public Message(int id, string format, dynamic variables = null, bool user = true, bool telemetry = false) {
        this.id = id;
        this.format = format;
        this.variables = variables;
        showUser = user;
        sendTelemetry = telemetry;
    }
}

public class StackFrame {
    public int id { get; }
    public Source source { get; }
    public int line { get; }
    public int column { get; }
    public string name { get; }
    public string presentationHint { get; }

    public StackFrame(int id, string name, Source source, int line, int column, string hint) {
        this.id = id;
        this.name = name;
        this.source = source;

        // These should NEVER be negative
        this.line = System.Math.Max(0, line);
        this.column = System.Math.Max(0, column);

        presentationHint = hint;
    }
}

public class Scope {
    public string name { get; }
    public int variablesReference { get; }
    public bool expensive { get; }

    public Scope(string name, int variablesReference, bool expensive = false) {
        this.name = name;
        this.variablesReference = variablesReference;
        this.expensive = expensive;
    }
}

public class Variable {
    public string name { get; }
    public string value { get; }
    public string type { get; }
    public int variablesReference { get; }

    public Variable(string name, string value, string type, int variablesReference = 0) {
        this.name = name;
        this.value = value;
        this.type = type;
        this.variablesReference = variablesReference;
    }
}

public class Thread {
    public int id { get; }
    public string name { get; }

    public Thread(int id, string name) {
        this.id = id;
        if (name == null || name.Length == 0) {
            this.name = string.Format("Thread #{0}", id);
        } else {
            this.name = name;
        }
    }
}

public class Source {
    public string name { get; }
    public string path { get; }
    public int sourceReference { get; }
    public string presentationHint { get; }

    public Source(string name, string path, int sourceReference, string hint) {
        this.name = name;
        this.path = path;
        this.sourceReference = sourceReference;
        presentationHint = hint;
    }
}

public class Breakpoint {
    public bool verified { get; }
    public int line { get; }

    public Breakpoint(bool verified, int line) {
        this.verified = verified;
        this.line = line;
    }
}