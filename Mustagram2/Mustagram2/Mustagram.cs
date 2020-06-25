using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;


// 회원가입
// 로그인
// 팔로우 언팔로우
// 개인정보 저장 수정
// 개인정보 가져오기
// 게시물 좋아요 갯수
// 댓글 좋아요 갯수
// ---------해야할 일---------
// 본문 CRUD
// 댓글 CRUD
// 친구목록 가져오기
// 친구 게시물 가져오기
// 게시물 댓글 가져오기
// 내 게시물 출력
// 팔로워 숫자 가져오기
// 유저 아이디 가져오기
namespace Mustagram2
{
    public class MustagramClient
    {
        private static HttpClient client = GenrateClient();
        private static MustagramClient mustagramClient = new MustagramClient();
        private MustagramClient() { }
        private static HttpClient GenrateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://ec2-18-191-128-120.us-east-2.compute.amazonaws.com:3000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            return client;
        }
        public static MustagramClient GetClient() { return mustagramClient; }
        public async Task<bool> UploadFilesAsync(string[] filePaths)
        {
            // 참고 링크
            // https://stackoverflow.com/questions/12968138/how-to-upload-a-file-in-window-forms
            // https://stackoverflow.com/questions/57690968/how-to-post-two-files-at-once-to-an-api-in-windows-forms-c-sharp
            var multiFormDataContent = new MultipartFormDataContent();

            foreach (string path in filePaths)
            {
                Console.WriteLine(path);

                FileStream fs = File.OpenRead(path);

                var streamContent = new StreamContent(fs);
                var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
                multiFormDataContent.Add(fileContent, "files", Path.GetFileName(path));
            }
            var response = await client.PostAsync("/file/upload", multiFormDataContent);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> SendUserPersonalInfo(
          string userId,
          string userName,
          string sex,
          string intro,
          string birthday
        )
        {
            var personalInfo = new
            {
                id = userId,
                name = userName,
                sex = sex,
                intro = intro,
                birthday = birthday
            };
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/user/personal/save", personalInfo
            );
            response.EnsureSuccessStatusCode();
            return isSuccess(await response.Content.ReadAsStringAsync());
        }
        public async Task<bool> SendUserModifiedPersonalInfo(
          string userId,
          string intro
        )
        {
            var personalInfo = new
            {
                id = userId,
                intro = intro,
            };
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/user/personal/modify", personalInfo
            );
            response.EnsureSuccessStatusCode();
            return isSuccess(await response.Content.ReadAsStringAsync());
        }
        public async Task<bool> SendLoginInfo(string userId, string userPassword)
        {
            
            var loginInfo = new
            {
                id = userId,
                password = userPassword
            };
            HttpResponseMessage response = await client.PostAsJsonAsync("/user/login", loginInfo).ConfigureAwait(false);
            Console.WriteLine("mus {0} {1}", userId, userPassword);
            Console.WriteLine(userId, userPassword);
            response.EnsureSuccessStatusCode();
           
            return isSuccess(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<bool> SendSignInInfo(string userId, string userPassword)
        {
            var signInInfo = new
            {
                id = userId,
                password = userPassword
            };
            HttpResponseMessage response = await client.PostAsJsonAsync("/user/sign-in", signInInfo);
            response.EnsureSuccessStatusCode();

            return isSuccess(await response.Content.ReadAsStringAsync());
        }
        public async Task<bool> SendFollowRequest(string myId, string friendId)
        {
            var followRequest = new
            {
                id = myId,
                friendId = friendId
            };
            HttpResponseMessage response = await client.PostAsJsonAsync("/user/follow", followRequest);
            response.EnsureSuccessStatusCode();

            return isSuccess(await response.Content.ReadAsStringAsync());
        }
        public async Task<bool> SendUnfollowRequest(string myId, string friendId)
        {
            var followRequest = new
            {
                id = myId,
                friendId = friendId
            };
            HttpResponseMessage response = await client.PostAsJsonAsync("/user/unfollow", followRequest);
            response.EnsureSuccessStatusCode();

            return isSuccess(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> GetPersonalDescription(string id)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/user/personal/get", new { id = id });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private class ResultType
        {
            public int like { get; set; }
        }
        public async Task<int> GetPostLike(int postNumber)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/post/count-like", new { postNumber = postNumber });
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<ResultType>();
            return result.like;
        }
        public async Task<int> GetCommentLike(int commentNumber)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/comment/count-like", new { commentNumber = commentNumber });
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<ResultType>();
            return result.like;
        }

        public async Task<String> GetStringAsync(string path)
        {
            string word = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
                word = await response.Content.ReadAsStringAsync();

            return word;
        }

        // 노드에서
        // 파일 입출력으로 Node에서 해당 파일을 삭제할 수 있도록 해야합니다.
        public async Task<bool> CreatePost(string id, string content)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync("/post/create", new { id = id, content = content });
            res.EnsureSuccessStatusCode();
            return isSuccess(await res.Content.ReadAsStringAsync());
        }

        public async Task<bool> UpdatePost(string id, int postNumber, string content)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync("/post/update", new { id = id, postNumber = postNumber, content = content });
            res.EnsureSuccessStatusCode();
            return isSuccess(await res.Content.ReadAsStringAsync());
        }

        public async Task<bool> DeletePost(string id, int postNumber)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync("/post/delete", new { id = id, postNumber = postNumber });
            res.EnsureSuccessStatusCode();
            return isSuccess(await res.Content.ReadAsStringAsync());
        }
        //

        private class Post
        {
            private int postNumber { get; set; }
            private int userNumber { get; set; }
            private string time { get; set; }
            private string content { get; set; }
        }

        private class PostList
        {

        }
        private bool isSuccess(String result) => result == "success";
    }
}