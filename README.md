NSocket
=======

A high performance .net socket communication libiary

Feature:
1. Can limit the max client quantity.
2. use connection pool to accept and manage clients.
3. Client disconnect detective
4. a. client postive disconnect, ex: client close it socket by himself.
   b. server postive disconnect, ex: interruption of power supply.
	
4. Use pre-allot for SAEA object to prevent memory fragmentation.

采用.net 异步socket的高性能服务器通讯库.


主要功能：
1. 设定最大连接数。
2. 采用连接池的形式实现客户端的Accept.
3. 客户端断线检测
	a. 客户端主动断线，服务器端收到ConnectionReset请求后会清理服务器端与客户端对应的Socket所使用的资源，并把SAEA对象放入IdlePool中。
	b. 服务器端主动监测，在规定的时间内未收到客户端的请求，则判断为客户端断线，这种情况主要适用于客户端非法关机，像断电等意外，客户端没法
通知服务器端要断开连接。
4. 采用预先分配的形式对SAEA所使用的Buffer进行初始化，防止内存碎片.
