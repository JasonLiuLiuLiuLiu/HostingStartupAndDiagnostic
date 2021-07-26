# HostingStartupAndDiagnostic

## HostingStartup  
在ASP.NET Core中我们可以使用一种机制来增强启动时的操作，它就是HostingStartup。如何叫"增强"操作，相信了解过AOP概念的同学应该都非常的熟悉。我们常说AOP使用了关注点分离的方式，增强了对现有逻辑的操作。而我们今天要说的HostingStartup就是为了"增强"启动操作，这种"增强"的操作甚至可以对现有的程序可以做到无改动的操作。例如，外部程序集可通过HostingStartup实现为应用提供配置服务、注册服务或中间件管道操作等。  

### HostStartup的定义

在类库中随便定义一个类并实现IHostingStartup接口，添加HostingStartupAttribute指定要启动的类的类型， 具体代码如下：

``` c#
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//通过HostingStartup指定要启动的类型
[assembly: HostingStartup(typeof(HostStartupWeb.HostingStartupInWeb))]
namespace HostStartupWeb
{
    public class HostingStartupInWeb : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            //程序启动时打印依据话，代表执行到了这里
            Debug.WriteLine("Web程序中HostingStartupInWeb类启动");

            //可以添加配置
            builder.ConfigureAppConfiguration(config => {
                //模拟添加一个一个内存配置
                var datas = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ServiceName", "HostStartupWeb")
                };
                config.AddInMemoryCollection(datas);
            });

            //可以添加ConfigureServices
            builder.ConfigureServices(services=> {
                //模拟注册一个PersonDto
                services.AddScoped(provider=>new PersonDto { Id = 1, Name = "yi念之间", Age = 18 });
            });

            //可以添加Configure
            builder.Configure(app => {
                //模拟添加一个中间件
                app.Use(async (context, next) =>
                {
                    await next();
                });
            });
        }
    }
}
```

### HostStartup的使用

把类库添加到当前项目的引用中，添加环境变量`ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`并指定其值为类库程序集名称。
以Demo中的代码为例：

``` xml
  <ItemGroup>
    <ProjectReference Include="..\HostingStartupAgent\HostingStartupAgent.csproj" />
  </ItemGroup>
```
``` json
    "HostingStartupAndDiagnostic": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "weatherforecast",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "HostingStartupAgent"
      }
    }
```

### 源码探究

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

### 生成Diagnostic 日志记录

如何生成 Diagnostic 日志记录呢？首先，我们需要创建一个 DiagnosticListener 对象，比如：  
``` c#
private static DiagnosticSource listener = new  DiagnosticListener("System.Net.Http");
```
`DiagnosticListener` 参数中的名称即为需要监听的事件（组件）名称，这个名称在以后会被用来被它的消费者所订阅使用, 然后这样来调用：

``` c#
if (listener.IsEnabled("RequestStart")){
    listener.Write("RequestStart", new { Url="http://clr", Request=aRequest });
}
```
`DiagnosticSource` 其核心只包含了两个方法，分别是 ：

``` c#
bool IsEnabled(string name)
void Write(string name, object value);
```
`IsEnabled(string param1)` 这个方法用来判断是否有消费者注册了当前的事件名称监听，通常有消费者关心了相关数据，我们才会进行事件记录。  
`Write(string param1,object param2)` 这个方法用来向 DiagnosticSource 中写入日志记录，param1 和上面一样用来指定名称的，也就是所向指定名称中写入数据，param2 即为写入的 payloads 数据，它是一个object,你可以使用 匿名类型来向 param2 中写入数据，这样会方便很多。

### 监听 Diagnostic 日志记录: 

在监听 Diagnostic 日志记录之前你需要知道你要关心的事件（组件）名称和事件名称, 以上面的例子为例，时间组件名称（listener.Name）是`System.Net.Http`,时间名称是`RequestStart`,那么监听 Diagnostic 日志记录的代码就可以这么写：  

``` c#

DiagnosticListener.AllListeners.Subscribe(delegate (DiagnosticListener listener)
{
    // 当 DiagnosticsListener 激活的时候，这里将获得一个回调用
    if (listener.Name == "System.Net.Http")
    {
            //回调业务代码
            Action<KeyValuePair<string, object>> callback = (KeyValuePair<string, object> evnt) =>
                Console.WriteLine("From Listener {0} Received Event {1} with payload {2}", networkListener.Name, evnt.Key, evnt.Value);
           
            //创建一个匿名Observer对象
            Observer<KeyValuePair<string, object>> observer = new AnonymousObserver<KeyValuePair<string, object>>(callback);
            
            //筛选你感兴趣的事件
            Predicate<string> predicate = (string eventName) => eventName == "RequestStart";
            
            listener.Subscribe(observer, predicate);
    }
});
```

那么如何知道第三方类库中已有的时间组件名称和事件名称呢？
在上面的代码`if (listener.Name == "System.Net.Http")`前加上`Console.WriteLine(listener.Name)`可以把所有的事件组件名称输出。  
在predicate中加上`Console.WriteLine(eventName)`可以把所有事件输出。  

Reference: https://www.cnblogs.com/savorboard/p/diagnostics.html
 
