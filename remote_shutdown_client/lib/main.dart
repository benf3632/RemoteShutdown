import 'package:flutter/material.dart';
import 'package:flutter/cupertino.dart';

Duration _time;

class RemoteShutdown extends StatefulWidget {
  @override
  _RemoteShutdownState createState() {
    return _RemoteShutdownState();
  }
}

class _RemoteShutdownState extends State<RemoteShutdown> {
  bool _started = false;
  int _radioMode = 0;

  @override
  Widget build(BuildContext context) {
    return new Scaffold(
      appBar: AppBar(
        title: const Text('Remote Shutdown'),
        centerTitle: true,
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
  
  void _start() {

  }

  void _stop() {

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
