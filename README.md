这是一个利用TCP为基础的远程监控程序。（开发语言C#，开发环境是VS2015）

能够获取摄像头截图，并且发送给客户端

整个项目分为客户端和服务端，客户端也叫接收端，服务端也叫发送端。
通过发送端调用电脑摄像头并且截图并且包装成字节数组，发送给客户端，客户端再把字节
数组解码成图片重新展示出来。中间的传输过程用了TCP协议来完成。
