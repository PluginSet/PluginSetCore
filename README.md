# 插件集 PluginSet

## 一、设计目的
插件集合设计时，基本理念是希望方便接入/移除SDK，在接入/移除SDK时，不需要写或少写新代码。同样功能的插件，可以由其它相同功能的插件替换，或同时生效，不需要或尽少修改业务逻辑。

---

## 二、设计思路
在核心插件```PluginSet.Core```中，定义了插件的生命周期和功能相关的接口，所有SDK或功能插件继承```PluginBase```并实现自身功能相关的接口后，通过增加```PluginRegister```类属性（只支持以```PluginSet```开头的程序集）或手动注册接口```PluginsManager.Regitster```注册到```PluginsManager```中，后者将对所有插件类进行实例化，对其生命周期进行管理（一般一个插件实例的生命周期由APP启动后开始到APP退出后结束），并统一了功能接口的调用方式。新增/移除插件不会对这些接口的实现产生影响。在使用插件时，一般不要直接使用插件接口，使用统一接口进行开发，以便插件的更替不会对业务层产生影响。
在核心插件编辑器代码```PluginSet.Core.Editor```中，对Android/iOS(```TODO```)导出流程进行了重新包装，每个插件都能通过特定的接口在导出流程中的某些阶段进行特殊处理，以便在导出结果中增加插件需要的特性。统一构建流程后，每次构建都能通过当前引用的所有插件与插件设置的相应参数，对导出结果产生影响。

---

### 三、主要实现
### 数据工具类 ```SerializedDataSet```
该类的作用主要是组合多个```ScriptableObject```，可以将不同的代码集中的数据结构```ScriptableObject```组合在一个主```ScriptableObject```中，自身作为主数据的子对象与主数据共同保存，方便管理各模块的数据参数。
PluginSet中主要运行的有两处：

* 编辑器下```BuildChannels``` 针对平台/渠道设置插件不同的参数
* 运行时```PluginSetConfig``` 插件在相应平台/渠道下运行时需要的参数，一般这些参数是设置在```BulidChannels```中，然后在构建时写入```PluginSetConfig```中


### 运行时插件基类 ```PluginBase```
每个运行时插件都需要继随自```PluginBase```，实例必须有明确唯一的名称，某些接口需要通过插件的```Name```属性来找到指定的插件实例。基类定义了**事件的监听添加/移除**接口，与**事件的派发**接口。插件间都是**通过事件机制来完成交互**的，以此来降低不同插件间的藕合。

派生类可以重载```Init(PluginSetConfig config)```方法，读取自身所需的配置来预设属性或初始化。该初始化方法只会在APP生命周期类调用一次。

### 运行时插件管理者 ```PluginsManager```
插件管理者```PluginsManager```在启动时，会对所有已注册的插件类进行创建/初始化调用，并在整个APP生命周期类，对所有插件进行相应的管理。
```PluginsManager```可以通过```Restart```接口进行重启，在重启过程中，插件可以通过实现接口来进行清理操作，来达到重启的效果。APP关闭时，也会调用到这些清理接口。

插件实例的功能接口不能直接调用，需要使用```PluginsManager```提示的共公接口来完成插件实例的功能调用。
下面根据不同的接口实现来介绍```PluginsManager```暴露执行插件功能的接口：

 * 按序启动 ```IStartPlugin```

   该接口可以用作异步初始化，或启动时需依赖其它插件初始化的情况，或在每次重启时需要清理数据时，或在启动/每次重启后需要重新初始化。插件的初始化顺序会根据```StartOrder```返回值，越小越优先初始化。```PluginsManager```会在组件的```Start```方法中，以及重启过程中按序调用所有实现了```IStartPlugin```接口的插件实例的```StartPlugin```方法。该方法返回的是异步过程，只有上一个插件异步执行结束之后，才会开始下一个插件的异步过程。```DisposePlugin```会在APP重启/关闭时被调用，调用顺序与```StartPlugin```方法一致（注1）。
 * 登录功能 ```ILoginPlugin```

   ```IsEnableLogin```方法控制插件登录接口启用状态，实现```Login```执行登录功能。```PluginManager```中可以通过```GetLoginTypes```方法来获取所有注册并这现了```ILoginPlugin```接口的插件名称，```GetValidLoginTypes```筛除未启用的实例。需要登录时，可以使用```LoginWithType```方法指定插件调用登录，也可以直接调用```Login```方法，该方法会抛出```”Notify_ChooseLoginType“```事件要求指定登录类型，业务逻辑中可监听该插件事件来弹出登录方式选择框，在玩家选择登录方式后调用到指定的登录方法。

 * 支付功能 ```IPaymentPlugin```

   ```IsEnablePayment```方法控制插件支付接口启用状态，实现```Pay```执行支付功能。```PluginManager```中可以通过```GetPayTypes```方法来获取所有注册并这现了```IPaymentPlugin```接口的插件名称，```GetValidPayTypes```筛除未启用的实例。需要支付时，可以使用```PayWithType```方法指定插件调用支付，也可以直接调用```Pay```方法，该方法会抛出```Notify_ChoosePayType```事件要求指定支付类型，业务逻辑中可监听该插件事件来弹出支付方式选择框，在玩家选择支付方式后调用到指定的支付方法。

 * 分享功能 ```ISharePlugin```

   ```IsEnableShare```方法控制插件分享接口启用状态，下列不同的分享功能共用该分享开关。分享调用方式与上述登录/支付逻辑类似，根据不同的分享类型，分享接口的具体定义不同，详细请参照接口定义。

 * 分享文本功能 ```IShareTextPlugin```
 * 分享图片功能 ```IShareImagePlugin```
 * 分享图片链接功能 ```IShareImageUrlPlugin```
 * 分享音乐功能 ```IShareMusicPlugin```
 * 分享音乐链接功能 ```IShareMusicUrlPlugin```
 * 分享视频功能 ```IShareVideoPlugin```
 * 分享视频链接功能 ```IShareVideoUrlPlugin```
 * 分享网页功能 ```IShareWebPagePlugin```
 * 分享小程序功能 ```IShareMiniProgramPlugin```

### 构建
为了能让各个插件能影响导出结果，插件集整合了构建流程，在部分阶段可以响应插件特定的方法对文件或数据进行修改。

#### 构建流程
构建过程中的相关数据储存在```BuildProcessorContext```类中，可以通过该实例的数据来修改导出数据。

```BuildProcessorContext```中的关键数据与方法
 * ```Symbols``` 所有用到的宏列表 可以在包含```OnSyncEditorSetting```属性的方法中增加所需的宏设置
 * ```LinkModules``` 包含的程序集 可以在包含```OnSyncEditorSetting```属性的方法中增加包含的程序集，插件集将根据该属性生成link.xml
 * ```TemplatePaths``` 临时文件夹列表，构建完成后将会清除这些文件夹
 * ```AddBuildBundle``` 可以在包含```OnBuildBundlesAttribute```或```OnBuildPatchesAttribute```属性的方法中通过```AddBuildBundle```方法来添加需要构建的AssetBundle及其中包含的文件

##### 数据准备
第一次打开项目时，需要使用"PluginSet->Init Plugins"菜单，生成默认的平台构建配置文件（在目录Assets/Editor/Channels中）

##### 构建准备
在编辑器下需要构建时，需要先在Build Settings中将平台设置为目标平台。设置好相应的平台配置，然后使用"PluginSet->Sync PluginsConfig"菜单，插件集将根据配置预设好Unity设置。

在使用命令行模式构建时，需要先使用命令切换至目标平台并调用```BuildHelper.PreBuild```方法，插件集将根据配置预设好Unity设置。在使用命令行时可以设置目标渠道，插件集将优先读取渠道相应的配置。

##### 构建APP
在做好构建准备后，编辑器下可以使用"PluginSet->Build Android Apk"来构建APP，但部分参数（比如渠道）无法预设。

使用命令行模式构建，在调用```BuildHelper.PreBuild```后，可以再调用```BuildHelper.Build```方法进行构建。

构建执行流程如下：

1. ```BuildInitialize``` 构建参数初始化
2. ```BuildSyncExportSetting``` 同步```PluginSetConfig```配置或其实临时数据
3. ```BuildPrepareGradleTemplates``` 整合所有插件所需的安卓项目配置，生成临时的安卓项目模版文件
4. ```BuildCheckExportedProject``` 检测是否需要重新导出项目（在同时多渠道打包时，有可能能减少导出项目的时间）
5. ```BuildPrepareBundles``` 准备构建AssetBundle资源，该流程中可以使用包含```OnBuildBundlesAttribute```属性的方法，来自定义AssetBundle构建的内容，也可以拷贝直接使用的资源到StreamingAssetsPath中。所有预设了```AssetBundleName```的资源都会使用```context.AddBuildBundle```方法加入到context中
6. ```BuildExportAssetBundles``` 构建导出AssetBundle资源，该流程根据所有加入context中的```BundleBuild```数据构建```AssetBundle```，构建完成后，会调用包含```OnBuildBundlesCompletedAttribute```属性的方法，可以在该方法中记录构建内容或修改构建结果
7. ```BuildExportProjectOrApk``` 导出工程或安卓APK（建议不直接导出APK），该方法会使用Unity提供的构建接口构建目标内容
8. ```BuildModifyAndroidProject``` 修改安卓工程（直接导出APK时没有该步骤），该流程可以使用包含```AndroidManifestModifyAttribute```属性的方法对AndroidManifest.xml文件进行修改，也可以使用包含```AndroidProjectModifyAttribute```属性的方法对安卓项目其它文件进行修改
9. ```BuildModifyIOSProject``` 修改iOS工程（TODO）
10. ```BuildGenerateApp``` 生成APP（直接导出APK时没有该步骤），安卓使用Gradle命令打包生成APK文件，构建完成后会调用包含```OnBuildAppCompletedAttribute```属性的方法
11. ```BuildEnd``` 构建结束，清理临时文件夹

##### 构建增量更新资源
编辑器下可以使用"PluginSet->Build Patches"菜单进行增量文件构建，但作用相当于重新构建所有AssetBundle资源。

命令行模式下调用```BuildHelper.BuildPatches```方法来进行热更新资源构建。

构建执行流程如下：
1. ```BuildInitialize``` 构建参数初始化
2. ```BuildPreparePatches``` 准备构建增量更新资源，该流程中可以使用包含```OnBuildPatchesAttribute```属性的方法，来自定义增量资源构建的内容，也可以拷贝直接使用的资源到StreamingAssetsPath中。所有预设了```AssetBundleName```的资源都会使用```context.AddBuildBundle```方法加入到context中
3. ```BuildExportAssetBundles``` 构建导出AssetBundle资源，该流程根据所有加入context中的```BundleBuild```数据构建```AssetBundle```，构建完成后，会调用包含```OnBuildBundlesCompletedAttribute```属性的方法，可以在该方法中记录构建内容或修改构建结果
4. ```BuildCopyBundles``` 将构建结果拷贝到```BuildPath```中
5. ```BuildEnd``` 构建结束，清理临时文件夹

***PS:在fabfile.py脚本中提供了通过git比对工具来收集文件差异，在命令行下调用```BuildHelper.BuildPatches```方法来根据文件差异构建所有AssetBundle资源，除了打包时包含的AssetBundle资源外，该方法还会根据文件差异，将有变化的```Assets/Resources```文件夹下的所有资源组织成可以通过ResourcesManager.Instance.Load加载到的AssetBundle中***

#### 构建相关属性 

属性|作用
---|---
```BuildToolsAttribute```|所有需要注入构建流程的类都需要增加该属性
```AndroidGradlePropertiesAttribute```|安卓```gradle.properties```文件中需要加入的选项，属性需返回一个```Dict<key:string, value:string>```，当有相同的```key```但```value```不同时，如果```priority```相同，则报错，如果```priority```不同，则使用较高权重的值
```AndroidExternalSourcesAttribute```|安卓项目```build.gradle```文件中需要加入的```EXTERNAL_SOURCES```, 属性需返回一个```List<name:string>```
```AndroidApplyPluginsAttribute```|安卓项目```build.gradle```文件中需要加入的```APPLY_PLUGINS```, 属性需返回一个```List<name:string>```
```AndroidBuildScriptAttribute```|安卓项目```build.gradle```文件中需要加入的```BUILD_SCRIPT_DEPS```，属性需返回一个```Dict<name:string, version:string>```，自动使用较高版本的依赖库，使用```string.Compare```来判断版本高低
```AndroidDependLibsAttribute```|安卓项目```build.gradle```文件中需要加入的远程依赖库```DEPS```，属性需返回一个```Dict<name:string, version:string>```，自动使用较高版本的依赖库，使用```string.Compare```来判断版本高低
```AndroidManifestModifyAttribute```|安卓项目设定```AndroidManifest.xml```时调用该接口
```AndroidMetadataAttribute```|安卓项目中需要加入Metadata的数据，方法需要返回一个```Dict<key:string, value:string>```结果
```AndroidProjectModifyAttribute```|安卓项目导出后对项目进行更改
```OnSyncEditorSettingAttribute```|重置编辑器设置（能保存的配置）
```OnSyncExportSettingAttribute```|重置编辑器设置（不能保存的编辑器配置，或会打入包内的内容）, 同步PluginsConfig
```OnCompileCompleteAttribute```|编译完成，第一次调用PostProcessScene时
```OnBuildBundlesAttribute```|准备构建AssetBundle时调用
```OnBuildPatchesAttribute```|准备构建热更资源时调用
```OnBuildBundlesCompletedAttribute```|AssetsBundle构建完成时调用
```OnBuildAppCompletedAttribute```|APP构建完成时调用

---

### 四、其它接口
#### ```ResourcesManager``` 资源管理接口
```PluginSet.Core```程序集中的```ResourcesManager```类是一个接口类（虚类），提供了资源加载的通用方法，需要在其它插件集中继承并实现加载接口。一个项目需要且只需要一个继承了```ResourcesManager```的程序集。

实现```ResourcesManager```的程序集需要根据需求考虑资源的构建方式与加载方式的配合。
例如已有```PluginSet.Patch```程序集中```PatchResourcesManager```继承了```ResourcesManager```，实现了资源加载的方法，并在```PluginResourceInit```中完成资源管理单例初始化，```PluginPatchUpdate```提供了相应的增量更新下载。同时在```PluginSet.Patch.Editor```中处理了AssetBundle的构建逻辑。

#### ```LoggerManager``` 日志管理
```LoggerManager```提供了分模块输出日志的方式，可以对不同模块设置不同的输出等级。

### ```Devices``` 系统接口
```Devices```用来统一不同平台调用系统方法的接口。
