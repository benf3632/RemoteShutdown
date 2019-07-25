import 'package:flutter/material.dart';
import 'package:flutter/cupertino.dart';
import 'socket.dart';
import 'package:fluttertoast/fluttertoast.dart';


Duration _time = new Duration(seconds: 0);

class RemoteShutdown extends StatefulWidget {
  @override
  _RemoteShutdownState createState() {
    return _RemoteShutdownState();
  }
}

class _RemoteShutdownState extends State<RemoteShutdown> {
  bool _started = false;
  int _radioMode = 0;
  String _ip;

  @override
  Widget build(BuildContext context) {
    return new Scaffold(
      appBar: AppBar(
        title: const Text('Remote Shutdown'),
        centerTitle: true,
        actions: <Widget>[
          new IconButton(
            icon: new Icon(Icons.help),
            onPressed: _showHelp,
          )
        ],
      ),
      body: new Center(
        child: new Column(
        children: <Widget>[
          new SizedBox(height: 16.0),
          new RaisedButton(
            onPressed: _showTimer,
            child: new Text('Select Time'), 
          ),
          new SizedBox(height: 16.0),
          new Text("Time Selected:",
                    style: new TextStyle(fontWeight: FontWeight.bold, fontSize: 20.0),),
          new Text(_time.toString().substring(0, _time.toString().indexOf('.')),style: new TextStyle(fontWeight: FontWeight.bold, fontSize: 20.0),),
          new SizedBox(height: 16.0),
          new Text('Mode:', style: new TextStyle(fontSize: 20.0),),
          new Row(
            children: <Widget>[
              new Radio(
                value: 0,
                onChanged: _changedMode,
                groupValue: _radioMode,
              ),
              new Text('Shutdown'),
              new Radio(
                value: 1,
                onChanged: _changedMode,
                groupValue: _radioMode,
              ),
              new Text('Restart'),
              new Radio(
                value: 2,
                onChanged: _changedMode,
                groupValue: _radioMode,
              ),
              new Text('Hibrnate'),
            ],
            mainAxisAlignment: MainAxisAlignment.center,
          ),
          new SizedBox(height: 16.0),
          new TextField(
            decoration: new InputDecoration(
              border: InputBorder.none,
              hintText: 'Enter IP to shutdown (click the ? for help)'
            ),
            onChanged: (ct) => _ip = ct,
            textAlign: TextAlign.center,            
          ),
          new SizedBox(height: 16.0),
          new RaisedButton(
            child: new Text(_started ? "Stop!" : "Start!"),
            onPressed: () {
              setState(() {
                _started = !_started;
              });
              if (_started) {
                _start();
              } else {
                _stop();
              }
            },
          )
        ],
        crossAxisAlignment: CrossAxisAlignment.center,
      ),
      ),
    );
  }
  
  void _start() async {
    try {
      Sock sock = new Sock();
      bool _connected = false;
      sock.connect(_ip);
      do {
        _connected = await sock.connected();
      } while (!_connected);

      List<int> buff = [];
      buff.add(100);
      buff.add(_radioMode);
      buff.addAll(sock.intToByte(_time.inSeconds));
      print(buff);
      sock.send(buff);

      bool recvAvail = false;
      do {
        recvAvail = await sock.available();
      } while (!recvAvail);
      List<int> recv = sock.recv();
      if (recv[0] == 50) {
        _showFailedToast();
      } else {
        _showSuccessToast();
      }
    }
    catch (e){
      _showFailedToast();
      print(e);
    }
  }

  void _stop() async{
    try {
      Sock sock = new Sock();
      bool _connected = false;
      sock.connect(_ip);
      do {
        _connected = await sock.connected();
      } while (!_connected);

      List<int> buff = [];
      buff.add(150);
      print(buff);
      sock.send(buff);

      bool recvAvail = false;
      do {
        recvAvail = await sock.available();
      } while (!recvAvail);
      List<int> recv = sock.recv();
      if (recv[0] == 50) {
        _showFailedToast();
      } else {
        _showStoppedToast();
      }
    }
    catch (e){
      _showFailedToast();
      print(e);
    }
  }

  void _showSuccessToast() {
    Fluttertoast.showToast(
      msg: "Timer started successfuly",
      textColor: Colors.white,
      toastLength: Toast.LENGTH_SHORT,
      timeInSecForIos: 1,
      gravity: ToastGravity.BOTTOM,
      backgroundColor: Colors.grey,
    );
  }

  void _showStoppedToast() {
    Fluttertoast.showToast(
      msg: "Timer stopped successfuly",
      textColor: Colors.white,
      toastLength: Toast.LENGTH_SHORT,
      timeInSecForIos: 1,
      gravity: ToastGravity.BOTTOM,
      backgroundColor: Colors.grey,
    );
  }

  void _showFailedToast() {
    Fluttertoast.showToast(
      msg: "Something went wrong",
      textColor: Colors.white,
      toastLength: Toast.LENGTH_SHORT,
      timeInSecForIos: 1,
      gravity: ToastGravity.BOTTOM,
      backgroundColor: Colors.grey,
    );
  }

  void _showHelp() {
    Navigator.push(context, MaterialPageRoute(builder: (context) => Help()));
  }

  void _changedMode(int type) {
    setState(() {
      _radioMode = type;
    });
  }

  void _showTimer() {
    Navigator.push(context, MaterialPageRoute(builder: (context) => TimerPicker()));
  }

}

class Help extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        title: new Text("Help"),
        centerTitle: true,
      ),
      body: new Row(
        children: <Widget>[
          new SizedBox(width: 20.0),
          new Text("To start the shutdown timer:\n1.pick a time with the button 'Pick Time'\n2.pick a mode (shutdown, restart, hibernate)\n3.Enter the IP of the computer to shutdown\n (can be seen in the option on the computer 'Show IP'\n4.Click Start\n\nNote: Need to be connected to the same network"),
        ], 
      ),
    );
  }
}


class MyApp extends StatelessWidget {
  const MyApp({Key key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: RemoteShutdown(),
    );
  }
}

class TimerPicker extends StatefulWidget {
  _TimerPickerState createState() => _TimerPickerState();
}

class _TimerPickerState extends State<TimerPicker> {
  @override
  Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        title: new Text('Pick time'),
        centerTitle: true,
      ),
      body: new CupertinoTimerPicker(
        onTimerDurationChanged: (Duration ct) {
          setState(() {
            _time = ct;
          });
        },
        minuteInterval: 1,
        secondInterval: 1,
      ),
    );
  }
}


void main() {
  runApp(MyApp());
}
