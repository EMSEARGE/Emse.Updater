#define CURL_STATICLIB
#include <stdio.h>
#include <curl/curl.h>
#include <curl/easy.h>
#include <string.h>
#include <stdlib.h>


#define false 0

size_t write_data(void *ptr, size_t size, size_t nmemb, FILE *stream) {
    size_t written;
    written = fwrite(ptr, size, nmemb, stream);
    return written;
}


int main(void) {
    CURL *curl;
    FILE *fp;
    CURLcode res;
    //char *url = "http://www.joes-hardware.com/tools.html";

    char *url= "https://curl.haxx.se/libcurl/c/allexamples.zip";
    //char *url=    "https://wordpress.org/";
    char outfilename[FILENAME_MAX] = "./json.zip";


    curl_version_info_data * vinfo = curl_version_info(CURLVERSION_NOW);

    if(vinfo->features & CURL_VERSION_SSL){
        printf("CURL: SSL enabled\n");
    }else{
        printf("CURL: SSL not enabled\n");
    }


    curl = curl_easy_init();
    if (curl) {
        fp = fopen(outfilename,"wb");
        curl_easy_setopt(curl, CURLOPT_URL, url);
        /* Setup the https:// verification options - note we do this on all requests as there may
           be a redirect from http to https and we still want to verify */
        //curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 1);
        curl_easy_setopt(curl, CURLOPT_CAINFO, "./ca-bundle.crt");
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, false);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, false);

        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_data);

        curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);
        res = curl_easy_perform(curl);
        curl_easy_cleanup(curl);


        int i=fclose(fp);
        if( i==0)
        system("unzip -j json.zip -d tempDir");
        system("rm -f json.zip");
        system("rm -rf /home/uurynk/App");
        system("cp -r tempDir/. /home/uurynk/App");
        system("rm -rf tempDir");
    }
    return 0;
}
