import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/genre.dart';
import '../models/api_response.dart';
import 'http_client_helper.dart';

class GenreService {
  final http.Client _client = HttpClientHelper.createHttpClient();

  // Lấy tất cả thể loại
  Future<ApiResponse<List<GenreListItem>>> getAllGenres() async {
    try {
      final response = await _client.get(
        Uri.parse('${ApiConfig.genresEndpoint}/all'),
        headers: {'Content-Type': 'application/json'},
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data is List) {
          return data.map((item) => GenreListItem.fromJson(item as Map<String, dynamic>)).toList();
        }
        return <GenreListItem>[];
      });

      return apiResponse;
    } catch (e) {
      return ApiResponse(
        status: false,
        message: 'Lỗi kết nối: ${e.toString()}',
        data: null,
      );
    }
  }

  // Lấy thể loại theo ID
  Future<ApiResponse<Genre?>> getGenreById(int id) async {
    try {
      final response = await _client.get(
        Uri.parse('${ApiConfig.genresEndpoint}/$id'),
        headers: {'Content-Type': 'application/json'},
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data != null) {
          return Genre.fromJson(data as Map<String, dynamic>);
        }
        return null;
      });

      return apiResponse;
    } catch (e) {
      return ApiResponse(
        status: false,
        message: 'Lỗi kết nối: ${e.toString()}',
        data: null,
      );
    }
  }
}

