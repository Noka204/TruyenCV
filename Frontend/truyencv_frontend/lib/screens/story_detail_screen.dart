import 'package:flutter/material.dart';
import '../models/story.dart';
import '../services/story_service.dart';
import '../services/author_service.dart';

class StoryDetailScreen extends StatefulWidget {
  final int storyId;

  const StoryDetailScreen({super.key, required this.storyId});

  @override
  State<StoryDetailScreen> createState() => _StoryDetailScreenState();
}

class _StoryDetailScreenState extends State<StoryDetailScreen> {
  final StoryService _storyService = StoryService();
  final AuthorService _authorService = AuthorService();
  Story? _story;
  String? _authorName;
  bool _isLoading = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _loadStory();
  }

  Future<void> _loadStory() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final response = await _storyService.getStoryById(widget.storyId);

    if (response.status && response.data != null) {
      final story = response.data!;
      setState(() {
        _story = story;
        _isLoading = false;
      });
      _loadAuthorName(story.authorId);
    } else {
      setState(() {
        _isLoading = false;
        _errorMessage = response.message;
      });
    }
  }

  Future<void> _loadAuthorName(int authorId) async {
    final response = await _authorService.getAuthorById(authorId);
    if (response.status && response.data != null) {
      setState(() {
        _authorName = response.data!.displayName;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Chi tiết Truyện'),
        backgroundColor: Colors.purple,
        foregroundColor: Colors.white,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _errorMessage != null
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.error_outline, size: 64, color: Colors.red),
                      const SizedBox(height: 16),
                      Text(
                        _errorMessage!,
                        style: const TextStyle(color: Colors.red),
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 16),
                      ElevatedButton(
                        onPressed: _loadStory,
                        child: const Text('Thử lại'),
                      ),
                    ],
                  ),
                )
              : _story == null
                  ? const Center(child: Text('Không tìm thấy truyện'))
                  : SingleChildScrollView(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (_story!.coverImage != null && _story!.coverImage!.isNotEmpty)
                            Center(
                              child: Image.network(
                                _story!.coverImage!,
                                height: 200,
                                fit: BoxFit.cover,
                                errorBuilder: (context, error, stackTrace) {
                                  return const Icon(Icons.image_not_supported, size: 100);
                                },
                              ),
                            ),
                          if (_story!.coverImage != null && _story!.coverImage!.isNotEmpty)
                            const SizedBox(height: 24),
                          Text(
                            _story!.title,
                            style: const TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 16),
                          _buildDetailRow('ID', _story!.storyId.toString()),
                          _buildDetailRow('Tác giả', _authorName ?? 'ID: ${_story!.authorId}'),
                          if (_story!.primaryGenreId != null)
                            _buildDetailRow('Thể loại chính', _story!.primaryGenreId.toString()),
                          _buildDetailRow(
                            'Trạng thái',
                            _story!.status,
                            valueColor: _story!.status == 'Hoàn thành'
                                ? Colors.green
                                : Colors.orange,
                          ),
                          if (_story!.description != null && _story!.description!.isNotEmpty)
                            _buildDetailRow('Mô tả', _story!.description!),
                          if (_story!.coverImage != null && _story!.coverImage!.isNotEmpty)
                            _buildDetailRow('URL Ảnh bìa', _story!.coverImage!),
                          _buildDetailRow('Ngày tạo', _formatDate(_story!.createdAt)),
                          _buildDetailRow('Ngày cập nhật', _formatDate(_story!.updatedAt)),
                        ],
                      ),
                    ),
    );
  }

  Widget _buildDetailRow(String label, String value, {Color? valueColor}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: const TextStyle(
              fontSize: 12,
              color: Colors.grey,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            style: TextStyle(
              fontSize: 16,
              color: valueColor,
            ),
          ),
          const Divider(),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year} ${date.hour}:${date.minute.toString().padLeft(2, '0')}';
  }
}

