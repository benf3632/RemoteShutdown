import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:flutter/gestures.dart';

class Help extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        title: new Text("Help"),
        centerTitle: true,
      ),
      body: new Column(
        children: <Widget>[
          new Row(
            children: <Widget>[
              new SizedBox(width: 20.0),
              new Text("To start the shutdown timer:\n1.pick a time with the button 'Pick Time'\n2.pick a mode (shutdown, restart, hibernate)\n3.Enter the IP of the computer to shutdown\n (can be seen in the option on the computer 'Show IP'\n4.Click Start\n\nNote: Need to be connected to the same network"),
            ], 
          ),
          new SizedBox(height: 16.0),
          new RichText(
            text: new TextSpan(
              children: [
                new TextSpan(
                  text: 'The Dekstop Application can be found at:\n',
                  style: new TextStyle(color: Colors.black)
                ),
                new TextSpan(
                  text: '     Github: RemoteShutdown-Releases',
                  style: new TextStyle(color: Colors.blue),
                  recognizer: new TapGestureRecognizer()
                    ..onTap = () { launch('https://github.com/benf3632/RemoteShutdown-Releases/releases');}
                )
              ]
            ),
          )
        ],
      )
    );
  }
}