import 'package:flutter/material.dart';
import '../models/story.dart';
import '../services/story_service.dart';
import '../services/author_service.dart';
import 'home_screen.dart';
import 'chapters_list_screen.dart';

class StoryDetailScreen extends StatefulWidget {
  final int storyId;

  const StoryDetailScreen({super.key, required this.storyId});

  @override
  State<StoryDetailScreen> createState() => _StoryDetailScreenState();
}

class _StoryDetailScreenState extends State<StoryDetailScreen> {
  final StoryService _storyService = StoryService();
  final AuthorService _authorService = AuthorService();
  final TextEditingController _searchController = TextEditingController();
  Story? _story;
  String? _authorName;
  bool _isLoading = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _loadStory();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _onSearchSubmitted(String value) {
    if (value.trim().isNotEmpty) {
      // Navigate về HomeScreen với query tìm kiếm
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (context) => HomeScreen(initialQuery: value.trim()),
        ),
      );
    }
  }

  Future<void> _loadStory() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final response = await _storyService.getStoryById(widget.storyId);

    if (response.status && response.data != null) {
      final story = response.data!;
      // Debug: kiểm tra coverImage
      debugPrint('Story detail coverImage: ${story.coverImage}');
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
      body:
          _isLoading
              ? const Center(child: CircularProgressIndicator())
              : _errorMessage != null
              ? Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      size: 64,
                      color: Colors.red,
                    ),
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
              : Column(
                children: [
                  // Thanh tìm kiếm
                  Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Container(
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.grey.withOpacity(0.2),
                            blurRadius: 4,
                            offset: const Offset(0, 2),
                          ),
                        ],
                      ),
                      child: TextField(
                        controller: _searchController,
                        decoration: InputDecoration(
                          hintText: 'Tìm kiếm truyện...',
                          prefixIcon: const Icon(
                            Icons.search,
                            color: Colors.grey,
                          ),
                          suffixIcon:
                              _searchController.text.isNotEmpty
                                  ? IconButton(
                                    icon: const Icon(
                                      Icons.clear,
                                      color: Colors.grey,
                                    ),
                                    onPressed: () {
                                      _searchController.clear();
                                      setState(() {});
                                    },
                                  )
                                  : null,
                          border: InputBorder.none,
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 12,
                          ),
                        ),
                        onSubmitted: _onSearchSubmitted,
                        onChanged: (value) => setState(() {}),
                      ),
                    ),
                  ),
                  // Nội dung chi tiết
                  Expanded(
                    child: SingleChildScrollView(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Ảnh bìa
                          if (_story!.coverImage != null &&
                              _story!.coverImage!.isNotEmpty)
                            Center(
                              child: Container(
                                margin: const EdgeInsets.only(bottom: 24),
                                decoration: BoxDecoration(
                                  borderRadius: BorderRadius.circular(12),
                                  boxShadow: [
                                    BoxShadow(
                                      color: Colors.grey.withOpacity(0.3),
                                      blurRadius: 8,
                                      offset: const Offset(0, 4),
                                    ),
                                  ],
                                ),
                                child: ClipRRect(
                                  borderRadius: BorderRadius.circular(12),
                                  child: Image.network(
                                    _story!.coverImage!,
                                    width: 200,
                                    height: 300,
                                    fit: BoxFit.cover,
                                    errorBuilder: (context, error, stackTrace) {
                                      return Container(
                                        width: 200,
                                        height: 300,
                                        color: Colors.grey.shade200,
                                        child: const Icon(
                                          Icons.image_not_supported,
                                          size: 60,
                                          color: Colors.grey,
                                        ),
                                      );
                                    },
                                    loadingBuilder: (
                                      context,
                                      child,
                                      loadingProgress,
                                    ) {
                                      if (loadingProgress == null) return child;
                                      return Container(
                                        width: 200,
                                        height: 300,
                                        color: Colors.grey.shade200,
                                        child: const Center(
                                          child: CircularProgressIndicator(),
                                        ),
                                      );
                                    },
                                  ),
                                ),
                              ),
                            ),
                          // Nút Đọc truyện
                          Container(
                            width: double.infinity,
                            margin: const EdgeInsets.only(bottom: 24),
                            child: ElevatedButton.icon(
                              onPressed: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder:
                                        (context) => ChaptersListScreen(
                                          storyId: _story!.storyId,
                                          storyTitle: _story!.title,
                                        ),
                                  ),
                                );
                              },
                              icon: const Icon(Icons.menu_book),
                              label: const Text(
                                'Đọc truyện',
                                style: TextStyle(
                                  fontSize: 18,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              style: ElevatedButton.styleFrom(
                                backgroundColor: Colors.purple,
                                foregroundColor: Colors.white,
                                padding: const EdgeInsets.symmetric(
                                  vertical: 16,
                                ),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                              ),
                            ),
                          ),
                          _buildDetailRow(
                            'Tác giả',
                            _authorName ?? 'ID: ${_story!.authorId}',
                          ),
                          _buildDetailRow(
                            'Trạng thái',
                            _story!.status,
                            valueColor:
                                _story!.status == 'Đã hoàn thành'
                                    ? Colors.green
                                    : Colors.orange,
                          ),
                          if (_story!.description != null &&
                              _story!.description!.isNotEmpty)
                            _buildDetailRow('Mô tả', _story!.description!),
                          _buildDetailRow(
                            'Ngày tạo',
                            _formatDate(_story!.createdAt),
                          ),
                          _buildDetailRow(
                            'Ngày cập nhật',
                            _formatDate(_story!.updatedAt),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
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
          Text(value, style: TextStyle(fontSize: 16, color: valueColor)),
          const Divider(),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year} ${date.hour}:${date.minute.toString().padLeft(2, '0')}';
  }
}
