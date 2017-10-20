#include "XMLParserHelper.h"
#include "../xmlParser/xmlParser.h"
#include <string>
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include "../App.h"

using namespace std;

extern string Domain;
extern string AppPath;
extern string AppName;


XMLParserHelper::XMLParserHelper() {
	// TODO Auto-generated constructor stub

}

XMLParserHelper::~XMLParserHelper() {
	// TODO Auto-generated destructor stub
}

void XMLParserHelper::ParseConfig()
{
	XMLNode xNode = XMLNode::openFileHelper("Configurations.xml");

	Domain = xNode.getChildNode("Settings").getAttribute("Domain");
	AppPath = xNode.getChildNode("Settings").getAttribute("AppPath");
	AppName = xNode.getChildNode("Settings").getAttribute("AppName");
}

