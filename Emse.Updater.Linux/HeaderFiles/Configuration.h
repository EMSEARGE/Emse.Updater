#ifndef CONFIGURATION_H_
#define CONFIGURATION_H_

#include <string>
#include <pugixml/pugixml.hpp>
#include <iostream>

struct ConfigurationDto
{
    std::string Domain;
    std::string HomePath;
    std::string AppPath;
    std::string AppName;
    int ThreadSleepCount;
};

struct ConfigurationDtoXml {
    struct Saver {
        template <typename T>
        void operator()(pugi::xml_node parent, std::string const& name, T const& value) const {
            auto node = named_child(parent, name);
            node.text().set(to_xml(value));
        }

        template <typename C>
        void operator()(pugi::xml_node parent, std::string const& name, std::string const& item_name, C const& container) const {
            auto list = named_child(parent, name);

            for (auto& item : container)
                operator()(list, item_name, item);
        }

        void operator()(pugi::xml_node parent, std::string const& name, ConfigurationDto const& o) const {
            auto dto = named_child(parent, name);
            operator()(dto, "Domain", o.Domain);
            operator()(dto, "HomePath", o.HomePath);
            operator()(dto, "AppPath", o.AppPath);
            operator()(dto, "AppName", o.AppName);
            operator()(dto, "ThreadSleepCount", o.ThreadSleepCount);
        }

    private:
        template <typename T> static T const& to_xml(T const& v) { return v; }
        static char const* to_xml(std::string const& v) { return v.c_str(); }

        pugi::xml_node named_child(pugi::xml_node parent, std::string const& name) const {
            auto child = parent.append_child();
            child.set_name(name.c_str());
            return child;
        }
    };

    struct Loader {
        void operator()(pugi::xml_node parent, std::string const& name, std::string& value) const {
            auto node = parent.first_element_by_path(name.c_str());
            value = node.text().as_string();
        }
        void operator()(pugi::xml_node parent, std::string const& name, int& value) const {
            auto node = parent.first_element_by_path(name.c_str());
            value = node.text().as_int();
        }

        template <typename C>
        void operator()(pugi::xml_node parent, std::string const& name, std::string const& item_name, C& container) const {
            auto list = parent.first_element_by_path(name.c_str());

            for (auto& node : list) {
                if (node.type() != pugi::xml_node_type::node_element) {
                    std::cerr << "Warning: unexpected child node type ignored\n";
                    continue;
                }
                if (node.name() != item_name) {
                    std::cerr << "Warning: unexpected child node ignored (" << node.name() << ")\n";
                    continue;
                }

                container.emplace_back();
                operator()(node, container.back());
            }
        }

        void operator()(pugi::xml_node dto, ConfigurationDto& o) const {
            operator()(dto, "Domain", o.Domain);
            operator()(dto, "HomePath", o.HomePath);
            operator()(dto, "AppPath", o.AppPath);
            operator()(dto, "AppName", o.AppName);
            operator()(dto, "ThreadSleepCount", o.ThreadSleepCount);
        }

        void operator()(pugi::xml_node parent, std::string const& name, ConfigurationDto& o) const {
            operator()(parent.first_element_by_path(name.c_str()), o);
        }
    };
};

#endif