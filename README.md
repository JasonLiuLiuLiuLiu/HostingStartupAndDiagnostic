# HostingStartupAndDiagnostic

## HostingStartup

``` c#
var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection();
if (!options.SuppressEnvironmentConfiguration)
{
    //添加环境变量
    configBuilder.AddEnvironmentVariables(prefix: "ASPNETCORE_");
}
//构建了_config实例
private readonly IConfiguration _config = configBuilder.Build();
```

``` c#
//承载启动是需要调用的HostingStartup程序集
public IReadOnlyList<string> HostingStartupAssemblies { get; set; }
//承载启动时排除掉不不要执行的程序集
public IReadOnlyList<string> HostingStartupExcludeAssemblies { get; set; }
//是否阻止HostingStartup启动执行功能，如果设置为false则HostingStartup功能失效
//通过上面的ExecuteHostingStartups方法源码可知
public bool PreventHostingStartup { get; set; }
//应用程序名称
public string ApplicationName { get; set; }

public WebHostOptions(IConfiguration configuration, string applicationNameFallback)
{
    ApplicationName = configuration[WebHostDefaults.ApplicationKey] ?? applicationNameFallback;
    HostingStartupAssemblies = Split($"{ApplicationName};{configuration[WebHostDefaults.HostingStartupAssembliesKey]}");
    HostingStartupExcludeAssemblies = Split(configuration[WebHostDefaults.HostingStartupExcludeAssembliesKey]);
    PreventHostingStartup = WebHostUtilities.ParseBool(configuration, WebHostDefaults.PreventHostingStartupKey);
}

//分隔配置的程序集信息,分隔依据为";"分号,这也是我们上面说过配置多程序集的时候采用分号分隔的原因
private IReadOnlyList<string> Split(string value)
{
    return value?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        ?? Array.Empty<string>();
}
```

``` c#
//承载启动是需要调用的HostingStartup程序集
public IReadOnlyList<string> HostingStartupAssemblies { get; set; }
//承载启动时排除掉不不要执行的程序集
public IReadOnlyList<string> HostingStartupExcludeAssemblies { get; set; }
//是否阻止HostingStartup启动执行功能，如果设置为false则HostingStartup功能失效
//通过上面的ExecuteHostingStartups方法源码可知
public bool PreventHostingStartup { get; set; }
//应用程序名称
public string ApplicationName { get; set; }

public WebHostOptions(IConfiguration configuration, string applicationNameFallback)
{
    ApplicationName = configuration[WebHostDefaults.ApplicationKey] ?? applicationNameFallback;
    HostingStartupAssemblies = Split($"{ApplicationName};{configuration[WebHostDefaults.HostingStartupAssembliesKey]}");
    HostingStartupExcludeAssemblies = Split(configuration[WebHostDefaults.HostingStartupExcludeAssembliesKey]);
    PreventHostingStartup = WebHostUtilities.ParseBool(configuration, WebHostDefaults.PreventHostingStartupKey);
}

//分隔配置的程序集信息,分隔依据为";"分号,这也是我们上面说过配置多程序集的时候采用分号分隔的原因
private IReadOnlyList<string> Split(string value)
{
    return value?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        ?? Array.Empty<string>();
}
```
Reference: https://www.cnblogs.com/wucy/p/14013622.html 

## Diagnostic

Producter:

``` c#
private static DiagnosticSource httpLogger = new  DiagnosticListener("System.Net.Http");
if (httpLogger.IsEnabled("RequestStart")){
    httpLogger.Write("RequestStart", new { Url="http://clr", Request=aRequest });
}
```

Comsumer: 

``` c#
static IDisposable networkSubscription = null;

// 使用 AllListeners 来获取所有的DiagnosticListeners对象，传入一个IObserver<DiagnosticListener> 回调
static IDisposable listenerSubscription = DiagnosticListener.AllListeners.Subscribe(delegate (DiagnosticListener listener)
{
    // 当 DiagnosticsListener 激活的时候，这里将获得一个回调用
    if (listener.Name == "System.Net.Http")
    {
        // 订阅者监听消费代码
        lock(allListeners)
        {
            if (networkSubscription != null)
                networkSubscription.Dispose();
            
            //回调业务代码
            Action<KeyValuePair<string, object>> callback = (KeyValuePair<string, object> evnt) =>
                Console.WriteLine("From Listener {0} Received Event {1} with payload {2}", networkListener.Name, evnt.Key, evnt.Value);
           
            //创建一个匿名Observer对象
            Observer<KeyValuePair<string, object>> observer = new AnonymousObserver<KeyValuePair<string, object>>(callback);
            
            //筛选你感兴趣的事件
            Predicate<string> predicate = (string eventName) => eventName == "RequestStart";
            
            networkSubscription = listener.Subscribe(observer, predicate);
        }
    }
});
```

Reference: https://www.cnblogs.com/savorboard/p/diagnostics.html
 
