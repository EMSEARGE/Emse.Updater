#include "../HeaderFiles/CurlHelper.h"

#define CURL_STATICLIB
#include <curl/curl.h>
#include <string.h>
#include <fstream>
#include <App.h>

#define false 0

size_t write_data(void *ptr, size_t size, size_t nmemb, FILE *stream) {
    size_t written;
    written = fwrite(ptr, size, nmemb, stream);
    return written;
}

const char* ConvertStrToConstChar(string str)
{
    char *cstr = new char[str.length() + 1];
    strcpy(cstr, str.c_str());
    return cstr;
}

CurlHelper::CurlHelper() {
    // TODO Auto-generated constructor stub

}

CurlHelper::~CurlHelper() {
    // TODO Auto-generated destructor stub
}

bool CurlHelper::DownloadFiles(string Domain)
{
    cout<<"Downloading files..."<<endl;
    CURL *curl;
    FILE *fp;
    CURLcode res;
    string command = Domain + App::LatestVersion + ".zip";
    const char *url= ConvertStrToConstChar(ConvertStrToConstChar(command));

    char outfilename[FILENAME_MAX] = "./tempDownloadedFile.zip";

    curl_version_info_data * vinfo = curl_version_info(CURLVERSION_NOW);

    if(vinfo->features & CURL_VERSION_SSL) printf("CURL: SSL enabled\n");
    else printf("CURL: SSL not enabled\n");

    curl = curl_easy_init();

    if (curl)
    {
        fp = fopen(outfilename,"wb");
        curl_easy_setopt(curl, CURLOPT_URL, url);
        curl_easy_setopt(curl, CURLOPT_CAINFO, "./ca-bundle.crt");
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, false);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, false);

        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_data);

        curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);
        res = curl_easy_perform(curl);
        curl_easy_cleanup(curl);


        int i=fclose(fp);
        if( i==0) return true;
    }

    return false;
}

bool CurlHelper::CheckVersion(string Domain)
{
    CURL *curl;
    FILE *fp;
    CURLcode res;

    string cmd = Domain + "version.txt";

    const char *url= ConvertStrToConstChar(ConvertStrToConstChar(cmd));
    char outfilename[FILENAME_MAX] = "./tempVersion.txt";

    curl_version_info_data * vinfo = curl_version_info(CURLVERSION_NOW);

    curl = curl_easy_init();

    if (curl)
    {
        fp = fopen(outfilename,"wb");
        curl_easy_setopt(curl, CURLOPT_URL, url);
        curl_easy_setopt(curl, CURLOPT_CAINFO, "./ca-bundle.crt");
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, false);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, false);

        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_data);

        curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);
        res = curl_easy_perform(curl);
        curl_easy_cleanup(curl);

        int i=fclose(fp);

        if( i==0)
        {
            string line;
            ifstream myfile ("tempVersion.txt");

            if (myfile.is_open())
            {
                getline (myfile,line);
                myfile.close();
            }

            system("rm -f tempVersion.txt");

            if(line == "")
            {
                return false;
            }
            else
            {
                App::LatestVersion = line;
                return true;
            }

        }
    }

    return false;
}