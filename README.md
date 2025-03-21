# xFileEncoder
 自定义文件头部加密

 使用[x-encrypt](https://github.com/Y-way/x-encrypt)(v3.2.0)库进行加密/解密

 # 使用方法

```sh
git clone --recursive https://github.com/Y-way/xFileEncoder.git

```
进入`xFileEncoder`目录,
使用 Visual Studio 打开XFileEncoder.sln工程

编译并测试

打开`xFileEncoder/XFileEncoder/Properties/launchSettings.json`文件

修改`commandLineArgs` 命令行参数测试加密/解密功能

```
Usage:

XFileEncoder command args

    command:执行命令,必须参数
        encrypt:加密命令
        decrypt:解密命令
    args:命令参数
        -load:加载欲加密文件,必须参数
        -out:输出文件名字.可选参数,默认文件名out.xfe
        -encrypt-size:加密数据长度,可选参数,默认16字节. 取值范围:Min(Clamp(encrypt-size, 1, 255), file_size)
        -encode-type:加密源数据方式,可选参数. none:只加密源文件内容, 默认;zip:加密并zip压缩源文件内容
        -help:查看帮助
    例:
    加密:
        XFileEncoder encrypt -load test.png -out test.png -encrypt-size 32 -encode-type zip
    解密:
        XFileEncoder edcrypt -load test.png -out test.png

```

**********

# 该项目已迁移到[x-encrypt](https://github.com/Y-way/x-encrypt/tree/main/C%23)
