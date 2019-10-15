import 'dart:io';

class Sock {
  Socket _socket;
  List<int> _data = [];
  bool _recived = false;
  bool _connected = false;

  void connect(String ip) {
    Socket.connect(ip, 8111).then((socket) {
      _socket = socket;
      _connected = true;
      print('Connected to: ${_socket.remoteAddress.address}:${_socket.remotePort}');
      _socket.listen((data) {_data = data; _recived = true;},
      onDone: () {
        _socket.close();
      });
    });
  }

  void send(List<int> buff) {
    _socket.add(buff);
  }

  Future<bool> connected() =>
    Future.delayed(Duration(milliseconds: 100), () => _connected);

  Future<bool> available() =>
    Future.delayed(Duration(milliseconds: 100), () => _recived);

  List<int> recv() {
    var data = _data;
    _data = null;
    _recived = false;
    return data;
  }

  List<int> intToByte(int n) {
    List<int> bytes = [];
    bytes.add(n >> 24);
    bytes.add(n >> 16);
    bytes.add(n >> 8);
    bytes.add(n & 0xFF);
    return bytes;
  }

}