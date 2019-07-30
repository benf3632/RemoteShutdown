import 'package:flutter/material.dart';
import 'package:get_ip/get_ip.dart';
import 'socket.dart';

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

  Future<Map> _getHosts() async{
    Map hosts = {};
    String ip = await GetIp.ipAddress;
    print(ip);
    List<String> ipSplit = ip.split('.');
    final String ipTemplate = ipSplit[0] + "." + ipSplit[1] + "." + ipSplit[2] + ".";
    List<int> msg = [25];

    for (int i = 0; i < 256; i++) {
      String address = ipTemplate + i.toString();
      print(address);
      Sock sock = new Sock();
      try {
        sock.connect(address);
        bool connected = false;
        int j = 1;
        do {
          connected = await sock.connected();
          j--;
        } while (connected == false && j > 0);
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
      }
      catch(e) {}
    }
    
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