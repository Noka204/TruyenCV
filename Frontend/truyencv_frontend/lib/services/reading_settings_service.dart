import 'package:shared_preferences/shared_preferences.dart';

class ReadingSettingsService {
  static final ReadingSettingsService _instance =
      ReadingSettingsService._internal();
  factory ReadingSettingsService() => _instance;
  ReadingSettingsService._internal();

  static const String _fontSizeKey = 'reading_font_size';
  static const double _defaultFontSize = 16.0;
  static const double _minFontSize = 12.0;
  static const double _maxFontSize = 24.0;

  double _fontSize = _defaultFontSize;
  bool _isInitialized = false;

  double get fontSize => _fontSize;
  double get minFontSize => _minFontSize;
  double get maxFontSize => _maxFontSize;

  Future<void> initialize() async {
    if (_isInitialized) return;

    final prefs = await SharedPreferences.getInstance();
    _fontSize = prefs.getDouble(_fontSizeKey) ?? _defaultFontSize;
    _isInitialized = true;
  }

  Future<void> setFontSize(double size) async {
    if (size < _minFontSize || size > _maxFontSize) return;

    _fontSize = size;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setDouble(_fontSizeKey, size);
  }

  Future<void> increaseFontSize() async {
    final newSize = (_fontSize + 2).clamp(_minFontSize, _maxFontSize);
    await setFontSize(newSize);
  }

  Future<void> decreaseFontSize() async {
    final newSize = (_fontSize - 2).clamp(_minFontSize, _maxFontSize);
    await setFontSize(newSize);
  }

  Future<void> resetFontSize() async {
    await setFontSize(_defaultFontSize);
  }
}

