import 'package:flutter/material.dart';
import 'package:get_ip/get_ip.dart';
import 'socket.dart';
import 'dart:io';

class Hosts extends StatefulWidget {
  Hosts({Key key}) : super(key: key);

  _HostsState createState() => _HostsState();
}

class _HostsState extends State<Hosts> {
  final GlobalKey<RefreshIndicatorState> _refreshIndicatorKey = new GlobalKey<RefreshIndicatorState>();

  String _ip = "";

  @override
  Widget build(BuildContext context) {
    return new Scaffold(
      body: new RefreshIndicator(
        key: _refreshIndicatorKey,
        child: _buildBody(),
        onRefresh: _refresh,
      ),
      appBar: new AppBar(
        title: new Text('Available Hosts'),
        centerTitle: true,
      ),
    );
  }

  Widget _buildBody() {
    return FutureBuilder(
      future:_getHosts(),
      builder: (BuildContext context, AsyncSnapshot snapshot) {
        if (snapshot.data != null) {
          Map hosts = snapshot.data;
          List<dynamic> keys = hosts.keys.toList();
          return ListView.builder(
            itemCount: hosts.length,
            itemBuilder: (context, i) {
              String key = keys[i];
              String hostName = hosts[key];
              return new Card(
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(100.0)),
                child: new InkWell(
                    onTap: () {
                        _ip = key;
                         _selectHost(context);
                    },
                        child: new Container(
                            width: 300,
                            height: 100,
                            child: new Column(
                                children: <Widget>[
                                    new SizedBox(height: 15.0,),
                                    new Text(hostName, style: new TextStyle(fontSize: 20.0, fontWeight: FontWeight.bold),),
                                    new SizedBox(height: 15.0,),
                                    new Text(key, style: new TextStyle(fontSize: 15.0),)
                                ],
                            ),
                     ),
                 ),
              );
            }
          );
        }
        return ListView();
      }
    );
  }


  Future<Map> _getHosts() async {
    Map hosts = {};
    String ip = await GetIp.ipAddress;
    final String subnet = ip.substring(0, ip.lastIndexOf('.'));
    final int port = 8111;
    List<int> msg = [25];

    for (int i = 1; i < 256; ++i) {
      final host = '$subnet.$i';

      try {
        final Socket s = await Socket.connect(host, port, timeout: new Duration(milliseconds: 5));
        s.destroy();
        s.close();
        Sock sock = new Sock();
        sock.connect(host);
        bool connected = false;
        print(host);
        do {
          connected = await sock.connected();
        } while (connected == false);
        if (connected != false) {
          bool avail = false;
          sock.send(msg);
          do {
            avail = await sock.available();
          } while (avail == false);

          List<int> recv = sock.recv();
          String recvStr = String.fromCharCodes(recv);
          List<String> host = recvStr.split(',');
          hosts[host[0]] = host[1];
        } 
      } catch (e) {
      }
    }
    print('done');
    return hosts;
  }

  Future<Null> _refresh() async{
    setState(() {
      _ip = "";
    });
    return null;
  }

  void _selectHost(BuildContext context) {
    Navigator.pop(context, _ip);
  }
}
