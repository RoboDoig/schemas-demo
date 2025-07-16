using Bonsai;
using System;
using RuleSchema;
using System.ComponentModel;
using System.Reactive.Linq;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class RuleSelector : Source<Rule>
{
    private Rule Rule;

    private string path = "";
    [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
    public string Path {
        get {
            return path;
        }
        set {
            path = value;

            Rule settings;
            using (var reader = new StreamReader(value)) {
                var parser = new MergingParser(new Parser(reader));

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                settings = deserializer.Deserialize<Rule>(parser);
            }

            Rule = settings;
            OnValueChanged(Rule);
        }
    }

    event Action<Rule> ValueChanged;

    void OnValueChanged(Rule value)
    {
        if (ValueChanged != null) {
            ValueChanged.Invoke(value);
        }
    }

    public override IObservable<Rule> Generate()
    {
        return Observable
            .Defer(() => Observable.Return(Rule))
            .Concat(Observable.FromEvent<Rule>(
                handler => ValueChanged += handler,
                handler => ValueChanged -= handler));;
    }
}