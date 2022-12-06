# -*- coding: utf-8 -*-
"""
fabfile
~~~~~~~~~~~~~~~~~~~

要使用 fab 命令，请在本地安装 Fabrile 2.1.x 及以上版本
"""

from distutils import archive_util
import os
import sys
import json
import logging
import hashlib
import zipfile
import shutil
import oss2
import qrcode
import requests
import git
import plistlib

from invoke import task
from invoke.exceptions import Exit

log = logging.Logger('fabric', level=logging.DEBUG)
log.addHandler(logging.StreamHandler(sys.stdout))


LogFiles = []


def SUCESS(tip):
    raise Exit(code=0, message="SUCCESS:%s" % tip)


def FAILURE(err, code=1):
    if len(LogFiles) > 0:
        file = LogFiles.pop()
        if os.path.exists(file):
            print('start print log file:: ', file)
            with open(file, 'r', encoding='UTF-8') as f:
                print("\n".join(f.readlines()))
            print('ended print log file:: ', file)
    raise Exit(code=code, message="FAILURE:%s" % err)


def get_unity_dict(filename):
    result = dict()
    with open(filename, 'r', encoding='UTF-8') as f:
        for line in f.readlines():
            kv = line.split(':')
            result[kv[0].strip()] = kv[1].strip()
    return result

def get_all_text(filename):
    with open(filename, 'r', encoding='UTF-8') as f:
        return f.read()

def get_file_first_line(filename):
    with open(filename, 'r', encoding='UTF-8') as f:
        return f.readline().strip()


def is_win_platform():
    return sys.platform.startswith('win')


UNITY_VALID_ASSETS_EXTENSIONS = ("txt", "bytes", "ttf", "json",
                                 "prefab", "unity", "anim", "controller",
                                 "mat", "spriteatlas", "asset", "shader",
                                 "png", "jpg", "bmp", "tif", "gif",
                                 "mp4", "mp3", "ogg", "wav")

STREAMINGASSETSNAME = "StreamingAssets"

# start init global params
WORK_PATH = os.getcwd()
PROTJECT_PATH = WORK_PATH
PROTJECT_ASSETS = os.path.join(PROTJECT_PATH, "Assets")
UNITY_HUB = os.environ.get("UNITY_HUB", None)
UNITY_PATH = os.environ.get("UNITY_PATH", None)
log.info("PROTJECT_PATH=\"%s\"" % (PROTJECT_PATH))
log.info("PROTJECT_ASSETS=\"%s\"" % (PROTJECT_ASSETS))
log.info("UNITY_HUB=\"%s\"" % (UNITY_HUB))
log.info("UNITY_PATH=\"%s\"" % (UNITY_PATH))
if UNITY_HUB:
    unity_version = get_unity_dict(os.path.join(PROTJECT_PATH, "ProjectSettings", "ProjectVersion.txt"))
    log.info("unity_version=\"%s\"" % (unity_version))
    path = os.path.join(UNITY_HUB, unity_version.get("m_EditorVersion", "unknow"))
    if os.path.exists(path):
        if is_win_platform():
            UNITY_PATH = os.path.join(path, "Unity.exe")
        else:
            UNITY_PATH = os.path.join(path, "Unity.app", "Contents", "MacOS", "Unity")
        log.info("Set UNITY_PATH to hub version " + UNITY_PATH)
    else:
        log.info("Cannot find file with %s" % (path))

if not UNITY_PATH or not os.path.exists(UNITY_PATH):
    FAILURE("请先设置正确的UNITY_PATH或UNITY_HUB目录")


def check_path(path):
    if os.path.exists(path):
        return True
    parent, _ = os.path.split(path)
    if check_path(parent):
        print('make dir ', path)
        os.mkdir(path)
        return True
    else:
        return FAILURE('cannot create path::' + path)


def execall(cmd):
    print('execall::: ', cmd)
    return os.system(cmd)


def rm_dir(dir):
    if os.path.exists(dir):
        shutil.rmtree(dir)


def rename_file(src, dst):
    if os.path.exists(src):
        os.rename(src, dst)


def rm_file(file):
    if os.path.exists(file):
        os.remove(file)


def file_md5(path):
    with open(path, 'rb') as f:
        sha = hashlib.md5(f.read()).hexdigest()
        return sha[0:6]


def copy_file(src, dst, md5=False):
    '''拷贝文件至目标文件，返回目标文件的全目录'''
    if not os.path.exists(src):
        return

    srcdir, srcname = os.path.split(src)
    dstdir, dstname = os.path.split(dst)
    check_path(dstdir)
    if md5:
        sha = file_md5(src)
        name_list = dstname.split('.')
        index_max = len(name_list) - 1
        new_name = '%s_%s.%s' % ('.'.join(name_list[0:index_max]), sha, name_list[index_max])
        dst = os.path.join(dstdir, new_name)
    shutil.copy(src, dst)
    return dst


def clear_ds_store(path):
    if os.path.isfile(path):
        if path.endswith('.DS_Store'):
            rm_file(path)
    else:
        for name in os.listdir(path):
            sub_path = os.path.join(path, name)
            if name == '.DS_Store':
                rm_file(sub_path)
            elif os.path.isdir(sub_path):
                clear_ds_store(sub_path)


def copy_path(src, dst, merge=False, md5=False, file_map=dict(), exclude_paths=set(), skip_files=set()):
    '''拷贝文件夹至目标文件夹中，如果merge为真则不会移除目录文件夹中多余的文件'''
    skip_files.add('.DS_Store')
    if not os.path.exists(src):
        print('not find path ', src)
        return
    check_path(dst)
    for name in os.listdir(src):
        from_path = os.path.join(src, name)
        to_path = os.path.join(dst, name)
        if os.path.isdir(from_path):
            if exclude_paths.__contains__(from_path):
                continue
            if not merge:
                rm_dir(to_path)
            copy_path(from_path, to_path, merge, md5, file_map, exclude_paths, skip_files)
        else:
            if exclude_paths.__contains__(from_path):
                continue
            if skip_files.__contains__(name):
                continue
            file_map[from_path] = copy_file(from_path, to_path, md5)


def copy_path_ex(src, dst, params, merge=False):
    '''拷贝文件夹至目标文件夹中，根据params修改文件名和文件内容，如果merge为真则不会移除目录文件夹中多余的文件'''
    if not os.path.exists(src):
        return
    for name in os.listdir(src):
        from_path = os.path.join(src, name)
        to_path = os.path.join(dst, name)
        if os.path.isdir(from_path):
            if not os.path.exists(to_path):
                os.mkdir(to_path)
            else:
                if not merge:
                    rm_dir(to_path)
            copy_path_ex(from_path, to_path, params, merge)
        else:
            with open(from_path, 'r', encoding='UTF-8') as f:
                try:
                    content = f.read()
                    if not content:
                        copy_file(from_path, to_path)
                    else:
                        with open(to_path, 'w', encoding='UTF-8') as w:
                            w.write(content)
                except Exception as identifier:
                    copy_file(from_path, to_path)


def parseJsonFile(path):
    '''解析JSON文件'''
    with open(path, 'r', encoding='UTF-8') as f:
        return json.load(f)
    return None


def resaveJsonFile(path):
    try:
        data = parseJsonFile(path)
        with open(path, 'w') as f:
            json.dump(data, f)
    except Exception as identifier:
        print(identifier)


def transformJsonDir(path):
    for name in os.listdir(path):
        sub_path = os.path.join(path, name)
        if os.path.isdir(sub_path):
            transformJsonDir(sub_path)
        else:
            resaveJsonFile(sub_path)


def encodeObjectParam(data):
    '''将字典转换为替换字符串'''
    result = str(data)
    result = result[1:len(result) - 1]
    li = result.split(',')
    return ',\n'.join(li)


def copy_file_with_alter(src_path, dst_path, alter_func, params):
    if not os.path.exists(src_path):
        return
    if not os.path.exists(dst_path):
        copy_file(src_path, dst_path)

    with open(src_path, 'r', encoding='UTF-8') as r:
        context = r.read()
        if alter_func:
            context = alter_func(context, params)

    if context:
        with open(dst_path, 'w', encoding='UTF-8') as w:
            w.write(context)
            print('write file %s success' % dst_path)


def get_all_path(path, result):
    for name in os.listdir(path):
        sub_path = os.path.join(path, name)
        if os.path.isdir(sub_path):
            get_all_path(sub_path, result)
        else:
            result.append(sub_path)


def zip_file(zip_path, zip_file):
    all_files = []
    get_all_path(zip_path, all_files)
    with zipfile.ZipFile(zip_file, 'w', zipfile.ZIP_DEFLATED) as f:
        for path in all_files:
            print('append zip file :: ', path)
            f.write(path, os.path.relpath(path, zip_path))
        f.close()

class ArchiveInfo(object):
    """<?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
        <plist version="1.0">
        <dict>
            <key>ApplicationProperties</key>
            <dict>
                <key>ApplicationPath</key>
                <string>Applications/barrett.app</string>
                <key>CFBundleIdentifier</key>
                <string>com.sagi.barrett</string>
                <key>CFBundleShortVersionString</key>
                <string>1.0.7</string>
                <key>CFBundleVersion</key>
                <string>8</string>
                <key>SigningIdentity</key>
                <string>Apple Development: Haifeng Deng (WA6MJB2Y9P)</string>
                <key>Team</key>
                <string>5KRL6VS2Z2</string>
            </dict>
            <key>ArchiveVersion</key>
            <integer>2</integer>
            <key>CreationDate</key>
            <date>2020-01-06T16:19:57Z</date>
            <key>Name</key>
            <string>Unity-iPhone</string>
            <key>SchemeName</key>
            <string>Unity-iPhone</string>
        </dict>
        </plist>"""
    path_application = ""
    bundle_id = ""
    version_name = ""
    version_code = 0
    cert = ""
    team_id = ""
    scheme = ""

    def from_archive_path(self, archive_path):
        info_path = os.path.join(archive_path, "Info.plist")
        if not os.path.exists(info_path):
            return
        with open(info_path, "rb") as f:
            plist = plistlib.load(f)

        application_properties = plist.get("ApplicationProperties")
        self.scheme = plist.get("SchemeName")

        self.path_application = application_properties.get("ApplicationPath")
        self.bundle_id = application_properties.get("CFBundleIdentifier")
        self.version_name = application_properties.get("CFBundleShortVersionString")
        self.version_code = application_properties.get("CFBundleVersion")
        self.cert = application_properties.get("SigningIdentity")
        self.team_id = application_properties.get("Team")
    

class BuildConfig(object):
    CONFIG_JSON_PATH = "build_config.json"

    path_project = ""
    path_project_ios = ""
    path_info_plist = ""

    build_types = None

    build_type = ""
    team_id = ""
    scheme = ""
    bundle_id = ""
    sign_style = ""
    bite_code = False
    method = ""
    profile = ""
    cert = ""
    configuration = ""
    version_name = ""
    version_code = 0
    build_num = 0

    name_ipa = ""

    path_build = ""
    path_output = ""
    path_tmp = ""

    def set_config(self, project, build_type, configuration, version_name, version_code, build_num):
        path, _ = os.path.split(project)
        self.path_project = path
        self.path_project_ios = project
        self.build_type = build_type
        self.configuration = configuration
        self.version_name = version_name
        self.version_code = version_code
        self.build_num = build_num

        self.path_output = os.path.join(self.path_project, "build")
        self.read_from_file()
        self.name_ipa = "_".join([self.bundle_id, self.version_name, str(self.build_num)]) + ".ipa"
        

    def read_from_file(self):
        path_config = os.path.join(self.path_project, self.CONFIG_JSON_PATH)

        if not os.path.exists(path_config):
            print('没有配置文件:' + path_config)
            return False

        with open(path_config, 'r') as f:
            json_obj = json.load(f)
            f.close()

        self.read_from_json(json_obj)

    def read_from_json(self, json_obj):
        path_info = json_obj.get("info")
        scheme = json_obj.get("scheme")
        build_types = json_obj.get("buildTypes")
        print(build_types, self.build_type)
        build_type_obj = build_types.get(self.build_type)

        self.path_info_plist = os.path.join(self.path_project, path_info)
        self.scheme = scheme
        self.team_id = build_type_obj.get("teamId")
        self.bundle_id = build_type_obj.get("bundleId")
        self.method = build_type_obj.get("method")
        self.sign_style = build_type_obj.get("signStyle")
        self.profile = build_type_obj.get("profile")
        self.cert = build_type_obj.get("cert")
        self.bite_code = build_type_obj.get("biteCode")

    def get_derived_data_path(self):
        return os.path.join(self.path_output, "tmp", str(self.build_num))

    def get_export_path(self):
        return os.path.join(self.path_output, str(self.build_num))

    def get_export_plist_path(self):
        return os.path.join(self.path_output, "tmp", str(self.build_num), self.bundle_id)

    def get_method(self):
        return self.method

    def generate_system_archive_path(self):
        """
        获得系统的 archive 文件夹,这样使用命令行导出后,在 xcode organizer 窗口一样可以看到
        :return:
        """
        username = os.environ.get('USER')
        group_dir = "_".join([self.bundle_id, self.version_name])
        filename = "_".join([self.bundle_id, self.version_name, str(self.build_num)])
        archive_path = "/Users/{username}/Library/Developer/Xcode/Archives/{group_dir}/{filename}.xcarchive".format(**locals())
        return archive_path

    def __str__(self):
        values = []
        for name, value in vars(self).items():
            values.append('%s=%s' % (name, value))

        return "\n".join(values)


class BuildToolIOS(object):
    build_config = None
    quiet = False
    dry = False

    def __init__(self, build_config, quiet, dry):
        self.build_config = build_config
        self.quiet = quiet
        self.dry = dry

    def ios_archive(self):
        code_sign_identity = self.build_config.cert
        provisioning_profile = self.build_config.profile

        project_path = self.build_config.path_project_ios
        scheme = self.build_config.scheme
        archive_path = self.build_config.generate_system_archive_path()
        derived_data_path = self.build_config.get_derived_data_path()
        configuration = self.build_config.configuration

        if project_path.endswith(".xcworkspace"):
            cmds = [
                "xcodebuild",
                "archive",
                "-workspace", project_path,
                "-scheme", scheme,
                "-archivePath", archive_path,
                "-configuration", configuration,
                "-derivedDataPath", derived_data_path,
                "-allowProvisioningUpdates",
            ]
        else:
            cmds = [
                "xcodebuild",
                "archive",
                "-project", project_path,
                "-scheme", scheme,
                "-archivePath", archive_path,
                "-configuration", configuration,
                "-derivedDataPath", derived_data_path,
                "-allowProvisioningUpdates",
            ]

        if code_sign_identity and provisioning_profile:
            # cmds.append("CODE_SIGN_IDENTITY='{code_sign_identity}'".format(**locals()))
            cmds.append("PROVISIONING_PROFILE_APP={provisioning_profile}".format(**locals()))
            # cmds.append("CODE_SIGN_STYLE=Manual")

        cmd = " ".join(cmds)
        print(cmd)
        self.run_print_result(cmd)
        return archive_path

    def ios_archive_export(self, archive_path, archive_info, export_path, method):
        """
        导出 archive
        :return path_ipa
        """

        archive_path = archive_path or ""
        export_path = export_path or ""

        scheme = archive_info.scheme
        version_name = archive_info.version_name
        version_code = archive_info.version_code

        export_plist_path = self.create_export_plist(method)

        cmds = [
            "xcodebuild",
            "-exportArchive",
            "-archivePath", archive_path,
            "-exportPath", export_path,
            "-exportOptionsPlist", export_plist_path,
            "-allowProvisioningUpdates",
        ]
        cmd = " ".join(cmds)
        print(cmd)
        self.run_print_result(cmd)
        result_plist = os.path.join(export_path, 'DistributionSummary.plist')
        if not os.path.exists(result_plist):
            return os.path.join(export_path, scheme + ".ipa")

        with open(result_plist, "rb") as f:
            plist = plistlib.load(f)
        ipa_names = list(plist.keys())
        ipa_name = None
        if len(ipa_names) > 0:
            ipa_name = ipa_names[0]
        if ipa_name == None:
            ipa_name = scheme + ".ipa"
        return os.path.join(export_path, ipa_name)

    def create_export_plist(self, method):
        plist_path = self.build_config.get_export_plist_path()
        plist_file = os.path.join(plist_path, 'export.plist')
        plist_dict = {
            "teamID": self.build_config.team_id,
            "signingStyle": self.build_config.sign_style,
            "signingCertificate": self.build_config.cert,
            "provisioningProfiles": {
                self.build_config.bundle_id: self.build_config.profile
            },
            "method": method,
            "compileBitcode": self.build_config.bite_code
        }
        print(plist_dict)
        if os.path.exists(plist_file):
            os.remove(plist_file)

        check_path(plist_path)
        with open(plist_file, "wb+") as f:
            plistlib.dump(plist_dict, f)
            f.close()

        return plist_file

    def clean(self):
        if os.path.exists(self.ipaPath):
            os.remove(self.ipaPath)
        if os.path.exists(self.tmpPath):
            cmds = [
                "rm",
                "-rf",
                self.tmpPath,
            ]
            if self.__execute(cmds) == 0:
                print('Clean temp dir success')
        if os.path.exists(self.build_path):
            cmds = [
                "rm",
                "-rf",
                self.build_path,
            ]
            if self.__execute(cmds) == 0:
                print('Clean dirs success')

    def archive(self):
        self.__prepare()
        archivePath = self.savePath + '/' + self.scheme + '.xcarchive'
        if os.path.exists(archivePath):
            execall("rm -rf " + archivePath)

        print('use scheme: %s' % self.scheme)
        print('use configuration: %s' % self.configuration)
        cmds = [
            'xcodebuild',
            'archive',
            "-scheme", self.scheme,
            "-configuration", self.configuration,
            "-derivedDataPath", self.build_path,
            "-archivePath", archivePath,
        ]
        if self.workspaceFile:
            cmds.append('-workspace')
            cmds.append(self.workspaceFile)
        if len(self.provisioning_profile_uuid) > 0:
            cmds.append("PROVISIONING_PROFILE_SPECIFIER=" + self.provisioning_profile_uuid)
        if len(self.certification_name) > 0:
            cmds.append("CODE_SIGN_IDENTITY=" + self.certification_name)
        if self.team_id:
            cmds.append("DEVELOPMENT_TEAM=" + self.team_id)
        if len(self.provisioning_profile_uuid) > 0 or len(self.certification_name) > 0 or self.team_id:
            cmds.append("CODE_SIGN_STYLE=Manual")
        if self.__execute(cmds) == 0:
            print("Archive project success!")

    def run_cmd(self, cmd):
        """
        运行脚本
        :return:
        """
        if not self.dry:
            return os.popen(cmd)

    def run_get_result(self, cmd):
        pipe = self.run_cmd(cmd)
        return pipe.read()

    def run_print_result(self, cmd):
        print("run:" + cmd)
        pipe = self.run_cmd(cmd)

        if not self.dry:
            while pipe and True:
                try:
                    line = pipe.readline()
                    if line:
                        print(line)
                    else:
                        break
                except Exception as e:
                    print(str(e))
                    pass
                    
# common ended==============================

# buildTarget Allows the selection of an active build target before loading a project. Possible options are:
# # standalone,
# # Win,
# # Win64,
# # OSXUniversal,
# # Linux,
# # Linux64,
# # LinuxUniversal,
# iOS,
# Android,
# # Web,
# # WebStreamed,
# # WebGL,
# # XboxOne,
# # PS4,
# # WindowsStoreApps,
# # Switch,
# # N3DS,
# # tvOS.

BUILD_TARGETS = {
    "ios": "iOS"
    , "android": "Android"
}


def call_unity_func(build_target, func_name, log_name, channel=None
                    , channelId=None, out_path=None, version_name=None
                    , build_number=None, patch_data=None, debug=False, product=False):
    buf = [
        UNITY_PATH
        , "-batchmode"
        , "-nographics"
        , "-quit"
        , "-projectPath"
        , os.path.relpath(PROTJECT_PATH, WORK_PATH)
        , "-buildTarget"
        , build_target
    ]
    if func_name is not None:
        buf.append("-executeMethod")
        buf.append(func_name)
    if log_name is not None:
        buf.append("-logFile")
        buf.append(log_name)
        LogFiles.append(log_name)
    if channel is not None:
        buf.append("-channel")
        buf.append(channel)
    if channelId is not None:
        buf.append("-channelId")
        buf.append(channelId)
    if out_path is not None:
        buf.append("-path")
        buf.append(out_path)
    if version_name is not None:
        buf.append("-versionname")
        buf.append(version_name)
    if build_number is not None:
        buf.append("-build")
        buf.append(build_number)
        buf.append("-versioncode")
        buf.append(build_number)
    if patch_data is not None:
        buf.append("-patchdata")
        buf.append(patch_data)
    if debug:
        buf.append("-debug")
    if product:
        buf.append("-product")

    cmd = " ".join(buf)
    return execall(cmd)

def build_unity(platform, log_path, **kwargs):
    build_target = BUILD_TARGETS.get(platform.lower(), None)
    if build_target is None:
        return FAILURE("暂不支持该平台(%s)导出" % platform)
    if call_unity_func(build_target
            , "PluginSet.Core.Editor.BuildHelper.PreBuild"
            , os.path.join(log_path, "prebuildLog") if log_path else None
            , **kwargs):
        return FAILURE("Unity prebuild fail!")
    if call_unity_func(build_target
            , "PluginSet.Core.Editor.BuildHelper.Build"
            , os.path.join(log_path, "buildLog") if log_path else None
            , **kwargs):
        return FAILURE("Unity build fail!")


def build_unity_patches(platform, log_path, **kwargs):
    build_target = BUILD_TARGETS.get(platform.lower(), None)
    if build_target is None:
        return FAILURE("暂不支持该平台(%s)导出" % platform)
    if call_unity_func(build_target
            , "PluginSet.Core.Editor.BuildHelper.PreBuild"
            , os.path.join(log_path, "prebuildLog") if log_path else None
            , **kwargs):
        return FAILURE("Unity prebuild fail!")
    if call_unity_func(build_target
            , "PluginSet.Core.Editor.BuildHelper.BuildPatch"
            , os.path.join(log_path, "patchLog") if log_path else None
            , **kwargs):
        return FAILURE("Unity build fail!")


def generateApk(android_project_path, debug):
    cmd = [
        "./gradlew",
        "-p",
        ".",
        "assembleDebug" if debug else "assembleRelease"
    ]
    execall("cd %s && %s" % (android_project_path, " ".join(cmd)))
    mode = "debug" if debug else "release"
    apk_name = "launcher-%s.apk" % mode
    build_apk_file = os.path.join(android_project_path, "launcher", "build", "outputs", "apk", mode, apk_name)
    return build_apk_file

def podInstall(project_path:str)->str:
    pod_file = os.path.join(project_path, "Podfile")
    if not os.path.exists(pod_file):
        return os.path.join(project_path, "Unity-iPhone.xcodeproj")

    execall("cd %s && pod install --repo-update" % project_path)
    return os.path.join(project_path, "Unity-iPhone.xcworkspace")


def generateIpa(ios_project_path:str, version_name:str, build_number:str, debug:bool, buildType="adHoc"):
    project = podInstall(ios_project_path)
    configuration = "debug" if debug else "release"
    build_config = BuildConfig()
    build_config.set_config(project, buildType, configuration, version_name, build_number, build_number)
    build_tool = BuildToolIOS(build_config, True, False)
    archive_path = build_tool.ios_archive()

    archive_info = ArchiveInfo()
    archive_info.from_archive_path(archive_path)

    export_path = build_config.get_export_path()
    ipa_path = build_tool.ios_archive_export(archive_path, archive_info, export_path, build_config.get_method())
    return ipa_path


def build_one(platform, channel, channelId, version_name, build_number, temp_path, out_path, debug, cache_log, product):
    check_path(temp_path)
    platform = platform.lower()
    try:
        build_unity(platform, temp_path if cache_log else None, version_name=version_name
                    , build_number=build_number, out_path=temp_path
                    , channel=channel, debug=debug, product=product, channelId=channelId)
    except Exit as e:
        error_path = os.path.join(out_path, "errors")
        check_path(error_path)
        rm_dir(error_path)
        check_path(error_path)
        shutil.move(temp_path, error_path)
        return FAILURE(e.message)
    except Exception as err:
        return FAILURE(err)
    
    LogFiles.clear()
    if platform == 'android':
        apk_file_name = generateApk(os.path.join(temp_path, channel), debug)
        if not os.path.exists(apk_file_name):
            return FAILURE("找不到构建的安卓APK" + apk_file_name)
        if out_path.endswith(".apk"):
            out_file = out_path
            (out_path, _) = os.path.split(out_file)
        else:
            apk_name = "%s-%s-v%s-%s.apk" % (channel, channelId, version_name.replace(".", "_"), build_number)
            out_file = os.path.join(out_path, apk_name)
        check_path(out_path)
        rm_file(out_file)
        copy_file(apk_file_name, out_file)
    elif platform == 'ios':
        ipa_file_name = generateIpa(os.path.join(temp_path, channel), version_name, build_number, debug, "adHoc")
        if not os.path.exists(ipa_file_name):
            return FAILURE("找不到构建的IPA:" + ipa_file_name)
        if out_path.endswith(".ipa"):
            out_file = out_path
            (out_path, _) = os.path.split(out_file)
        else:
            ipa_name = "%s-%s-v%s-%s.ipa" % (channel, channelId, version_name.replace(".", "_"), build_number)
            out_file = os.path.join(out_path, ipa_name)
        check_path(out_path)
        rm_file(out_file)
        copy_file(ipa_file_name, out_file)


def uploadFileToOss(filename, path, bucket, force=False):
    if filename.endswith(".DS_Store"):
        return
    # 上传文件保持路径全小写
    path = path.lower()
    if not force and bucket.object_exists(path):
        return print("filename %s at path %s is exists!" % (filename, path))
    bucket.put_object_from_file(path, filename)
    print("uploadFileToOss :::: ", filename, path)
    if not bucket.object_exists(path):
        return FAILURE("上传文件%s失败" % filename)


def uploadDirToOss(dir, path, bucket, types=None, exclude=None):
    for filename in os.listdir(dir):
        d = os.path.join(dir, filename)
        if os.path.isdir(d):
            uploadDirToOss(d, path + "/" + filename, bucket, types, exclude)
        else:
            skip = False
            if types is not None:
                for type in types:
                    if not d.endswith(type):
                        skip = True
                        break
            if exclude is not None:
                for exc in exclude:
                    if d.endswith(exc):
                        skip = True
                        break
            if skip:
                continue
            uploadFileToOss(d, path + "/" + filename, bucket)


def get_patch_tag(platform, version_name, root):
    return "%s_%s_%s" % (root, platform, version_name)


def check_is_assets(filename):
    if not filename.startswith("assets/"):
        return False
    if filename.__contains__("/editor/"):
        return False
    if filename.__contains__("/resources/") and not filename.startswith("assets/resources/"):
        return False
    ext = filename.split('.')[-1]
    if not UNITY_VALID_ASSETS_EXTENSIONS.__contains__(ext):
        return False
    return True


def build_patches(platform, version_name, build_number, out_path, root, debug, cache_log):
    tag_name = get_patch_tag(platform, version_name, root)
    target_tag = None
    repo = git.Repo(PROTJECT_PATH)
    for tag in repo.tags:
        if tag.name == tag_name:
            target_tag = tag
            break
    if target_tag is None:
        return FAILURE("Cannot find tag with name " + tag_name)
    head = repo.head.commit
    diff = target_tag.commit.diff(head)
    add_files = []
    change_files = []
    for add in diff.iter_change_type("A"):
        filename = add.b_path.lower()
        if filename.endswith(".meta"):
            continue
        if not check_is_assets(filename):
            continue
        add_files.append(os.path.join(PROTJECT_PATH, filename))
    for mod in diff.iter_change_type("M"):
        filename = mod.b_path.lower()
        if filename.endswith(".meta"):
            filename = filename[0:-5]
        if not check_is_assets(filename):
            continue
        change_files.append(os.path.join(PROTJECT_PATH, filename))
    for ren in diff.iter_change_type("R"):
        filename = ren.b_path.lower()
        if filename.endswith(".meta"):
            filename = filename[0:-5]
        if not check_is_assets(filename):
            continue
        if ren.a_blob == ren.b_blob:
            add_files.append(os.path.join(PROTJECT_PATH, filename))
        else:
            change_files.append(os.path.join(PROTJECT_PATH, filename))
    patchdata = {
        "AddFiles": list(set(add_files)),
        "ModFiles": list(set(change_files)),
    }
    print("patchdata::::::\n%s" % json.dumps(patchdata, indent="  "))
    temp_path = os.path.join(PROTJECT_PATH, "Build", "patches", platform)
    rm_dir(temp_path)
    try:
        build_unity_patches(platform, temp_path if cache_log else None,
                            version_name=version_name, out_path=out_path, build_number=build_number,
                            patch_data="'%s'" % json.dumps(patchdata),debug=debug)
    except Exit as e:
        error_path = os.path.join(out_path, "errors")
        check_path(error_path)
        rm_dir(error_path)
        check_path(error_path)
        shutil.move(temp_path, error_path)
        return FAILURE(e.message)
    except Exception as err:
        return FAILURE(err)


@task(help={"path": "新项目名称"})
def create(context, path):
    execall(" ".join([
        UNITY_PATH
        , "-batchmode"
        , "-quit"
        , "-createProject"
        , path
    ]))


@task(help={
    "platform": "编译目标平台，目前支持ios与android",
    "channel": "目标渠道",
    "channelId": "目标渠道ID",
    "version_name": "版本号名称",
    "build_number": "build号",
    "out_path": "输出目录",
    "debug": "DEBUG模式会打开构建时的开发模式选项，且增加DEBUG宏",
    "log": "保存log文件",
    "product": "是否为生产模式",
})
def buildApp(context, platform, channel, channelId, version_name, build_number, out_path, debug=False, log=True, product=False):
    temp_path = os.path.join(PROTJECT_PATH, "Build", platform, "build_%s" % build_number)
    rm_dir(temp_path)
    build_one(platform, channel, channelId, version_name, build_number, temp_path, out_path, debug, log, product)
    return SUCESS("Build Completed")


@task(help={
    "platform": "编译目标平台，目前支持ios与android",
    "channel": "目标渠道",
    "channelIds": "目标渠道ID，多渠道ID以逗号','隔开",
    "version_name": "版本号名称",
    "build_number": "build号",
    "out_path": "输出目录",
    "debug": "DEBUG模式会打开构建时的开发模式选项，且增加DEBUG宏",
    "log": "保存log文件",
    "product": "是否为生产模式",
})
def buildMultiApp(context, platform, channel, channelIds, version_name, build_number, out_path, debug=False, log=True, product=False):
    temp_path = os.path.join(PROTJECT_PATH, "Build", platform, "build_%s" % build_number)
    rm_dir(temp_path)
    for c in channelIds.split(','):
        build_one(platform, channel, c.strip(), version_name, build_number, temp_path, out_path, debug, log, product)
    return SUCESS("Build Completed")


@task(help={
    "platform": "编译目标平台，目前支持ios与android",
    "version_name": "版本号名称",
    "build_number": "build号",
    "out_path": "输出目录",
    "debug": "DEBUG模式会打开构建时的开发模式选项，且增加DEBUG宏",
    "root": "上传至的根目录(环境标识)",
    "log": "保存log文件",
})
def buildPatches(context, platform, version_name, build_number, out_path, root, debug=False, log=True):
    build_target = BUILD_TARGETS.get(platform.lower(), None)
    if build_target is None:
        return FAILURE("暂不支持该平台(%s)导出" % platform)
    out_path = os.path.abspath(out_path)
    build_patches(platform, version_name, build_number, out_path, root, debug, log)
    return SUCESS("Build Patches Success in :" + out_path)


@task(help={
    "platform": "编译目标平台，目前支持ios与android",
    "version_name": "版本号",
    "commit_id": "TAG对应的commit sha值",
    "root": "上传至的根目录(环境标识)",
})
def addPatchTag(context, platform, version_name, commit_id, root):
    repo = git.Repo(PROTJECT_PATH)
    try:
        repo.tree(commit_id)
    except Exception as err:
        return FAILURE("Cannot find commit with commit_id " + commit_id)
    tag_name = get_patch_tag(platform, version_name, root)
    for tag in repo.tags:
        if tag.name == tag_name:
            repo.delete_tag(tag)
    repo.create_tag(tag_name, commit_id)

@task(help={
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    'file': "需要上传的本地文件目录",
    'key': "需要上传至的路径（包括文件名）",
    'endpoint': "oss域名，默认为杭州点",
})
def uploadFile(context, id, secret, bucketname, file, key, endpoint="https://oss-cn-hangzhou.aliyuncs.com"):
    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    uploadFileToOss(file, key, bucket, True)
              

@task(help={
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    "file": "当前编译出的ipa文件路径",
    'key': "需要上传至的目录）",
    'endpoint': "oss域名，默认为杭州点",
    'cname': "自定义域名（该域名可能没有写入权限）",
})
def buildIpaInstaller(context, id, secret, bucketname, file, key, cname=None
              , endpoint="https://oss-cn-hangzhou.aliyuncs.com"):
    pass
    path, _ = os.path.split(file)
    temp_path = os.path.join(path, "temp")

    url_prefix = cname
    if url_prefix is None:
        address = endpoint.replace("http://", "")
        address = address.replace("https://", "")
        url_prefix = "http://%s.%s" % (bucketname, address)
    remote = "%s/%s" % (url_prefix, key)

    build_target = BUILD_TARGETS.get("ios", None)
    buf = [
        UNITY_PATH
        , "-batchmode"
        , "-nographics"
        , "-quit"
        , "-projectPath"
        , os.path.relpath(PROTJECT_PATH, WORK_PATH)
        , "-buildTarget"
        , build_target
    ]
    buf.append("-executeMethod")
    buf.append("PluginSet.Core.Editor.BuildHelper.BuildIpaInstaller")
    buf.append("-ipa")
    buf.append(file)
    buf.append("-output")
    buf.append(temp_path)
    buf.append("-remote")
    buf.append(remote)

    cmd = " ".join(buf)
    execall(cmd)

    if not os.path.exists(temp_path):
        return FAILURE("生成IPA安装器失败")

    qr = qrcode.QRCode(
        version=5,
        error_correction=qrcode.constants.ERROR_CORRECT_L,
        box_size=10,
        border=4,
    )
    qr.add_data("%s/index.html" % remote)
    qr.make(fit=True)
    img = qr.make_image()
    filename = 'qrcode_temp.png'
    img.save(os.path.join(temp_path, filename))

    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    uploadDirToOss(temp_path, key, bucket)
    shutil.rmtree(temp_path)


@task(help={
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    'file': "需要上传的本地文件目录",
    'key': "需要上传至的目录，如果不以.apk结尾，则文件名与原文件名相同",
    'endpoint': "oss域名，默认为杭州点",
    'cname': "自定义域名（该域名可能没有写入权限）",
    'qrpath': "不为空时会上传下载地址的二维码",
})
def uploadApk(context, id, secret, bucketname, file, key
              , endpoint="https://oss-cn-hangzhou.aliyuncs.com"
              , cname=None, qrpath=None):
    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    if not key.endswith(".apk"):
        (filepath, filename) = os.path.split(file)
        key = "%s/%s" % (key, filename)
    uploadFileToOss(file, key, bucket, True)
    if qrpath is not None:
        url_prefix = cname
        if url_prefix is None:
            address = endpoint.replace("http://", "")
            address = address.replace("https://", "")
            url_prefix = "http://%s.%s" % (bucketname, address)
        url = "%s/%s" % (url_prefix, key)
        qr = qrcode.QRCode(
            version=5,
            error_correction=qrcode.constants.ERROR_CORRECT_L,
            box_size=10,
            border=4,
        )
        qr.add_data(url)
        qr.make(fit=True)
        img = qr.make_image()
        filename = 'qrcode_temp.png'
        img.save(filename)
        bucket.put_object_from_file(qrpath, filename)
        rm_file(filename)
    return SUCESS("上传文件%s成功" % file)


@task(help={
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    'file': "需要上传的本地文件目录",
    'key': "需要上传至的目录",
    'endpoint': "oss域名，默认为杭州点",
})
def uploadApks(context, id, secret, bucketname, file, key, endpoint="https://oss-cn-hangzhou.aliyuncs.com"):
    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    uploadDirToOss(file, key, bucket, types=[".apk"])
    return SUCESS("上传文件%s成功" % file)


@task(help={
    'version_name': "版本号",
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    'file': "需要上传的本地文件目录",
    'key': "需要上传至的目录",
    "build": "build号",
    'endpoint': "oss域名，默认为杭州点",
})
def uploadPatches(context, version_name, id, secret, bucketname, file, key, build,
                  endpoint="https://oss-cn-hangzhou.aliyuncs.com"):
    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    uploadDirToOss(file, key, bucket, exclude=[".meta", ".manifest"])
    return SUCESS("上传文件夹%s成功" % file)


def writeVersion(name, version_name, id, secret, bucketname, upload_keys, key
                 , build, cname, endpoint="https://oss-cn-hangzhou.aliyuncs.com"):
    file = "version.manifest"
    data = {
        "packageUrl": "%s/%s/" % (cname, key),
        "streamingAsset": "%s_v%s-%s" % (name, version_name, build),
        "version": version_name,
        "build": int(build),
    }
    with open(file, 'w', encoding='UTF-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print("writeVersionFile content:: ", get_all_text(file))
    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    for upload_key in upload_keys:
        uploadFileToOss(file, upload_key, bucket, True)
    rm_file(file)
    

@task(help={
    'platform': "平台",
    'version_name': "版本号",
    "channels": "目标渠道，多渠道以逗号','隔开",
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    'root': "热更文件根目录",
    'key': "热更文件下载目录",
    "build": "build号",
    'endpoint': "oss域名，默认为杭州点",
    'cname': "下载地址头",
    'patches': "子包名称列表",
})
def writeVersionFile(context, platform, channels, version_name, id, secret, bucketname, root, key
                     , build, cname, endpoint="https://oss-cn-hangzhou.aliyuncs.com", patches=None):
    file = "version.manifest"
    upload_keys = []
    for c in channels.split(','):
        upload_key = "%s/%s/%s/%s-%s/%s" % (root, platform, c.strip(), version_name, build, file)
        upload_keys.append(upload_key)
        
    writeVersion(STREAMINGASSETSNAME.lower(), version_name, id, secret, bucketname, upload_keys, key
                 , build, cname, endpoint)
    
    if patches:
        for patch in patches.split(','):
            upload_keys = []
            for c in channels.split(','):
                upload_key = "%s/%s/%s/%s/%s-%s/%s" % (root, platform, patch.lower(), c.strip(), version_name, build, file)
                upload_keys.append(upload_key)
            writeVersion(patch.lower(), version_name, id, secret, bucketname, upload_keys, key
                         , build, cname, endpoint)
        


@task(help={
    'webhook': "钉钉webhook url",
    'title': "标题内容",
    'markdown': "显示内容（makrdown格式）",
    'atmobiles': "需要AT的手机号，多个用逗号隔开",
    'isatall': "是否AT所有人",
})
def sendDingTalk(content, webhook, title, markdown, atmobiles=None, isatall=False):
    data = {
        "msgtype": "markdown",
    }

    phone_str = ""
    if atmobiles is not None:
        numbers = atmobiles.split(",")
        data.update({"at": {
            "atMobiles": numbers,
            "isAtAll": False
        }})
        phone_str = "@%s" % " @".join(numbers)
    data.update({"markdown": {
        "title": title,
        "text": "### %s  %s  \n\n%s" % (title, phone_str, markdown.replace('\\n', '\n'))
    }})
    if isatall:
        at = data.get("at", None)
        if at is not None:
            at.update({"isAtAll": True})
    request_data = json.dumps(data, ensure_ascii=False)
    request = requests.post(webhook, data=request_data.encode("utf-8"), headers={"Content-Type": "application/json"})
    print("ok=", request.ok)
    print("text=", request.text)
    print("reason=", request.reason)
    # if not request.ok:
    #     return FAILURE(request.reason)
    # resp = json.loads(request.text)
    # code = resp.get('errcode')
    # if code != 200:
    #     return FAILURE(resp.get("errmsg", "Unknow Error"))
    return SUCESS("发送成功")


@task(help={
    'id': "登录使用的AccessKeyId",
    'secret': "登录使用的AccessKeySecret",
    'bucketname': "域名（无需oss前缀）",
    'key': "需要上传至的全路径",
    'url': "下载app的页面地址"
})
def makeQR(context, id, secret, bucketname, endpoint, key, url):
    qr = qrcode.QRCode(version=5, error_correction=qrcode.constants.ERROR_CORRECT_L, box_size=10, border=4)
    qr.add_data(url)
    qr.make(fit=True)
    img = qr.make_image()
    filename = 'qrcode_temp.png'
    img.save(filename)
    auth = oss2.AuthV2(id, secret)
    bucket = oss2.Bucket(auth, endpoint, bucketname)
    bucket.put_object_from_file(key, filename)
    rm_file(filename)
    return SUCESS("上传文件%s成功" % key)

