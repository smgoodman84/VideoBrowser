//*
var filedata = [];
var basedir = "C:\\Users\\Simon\\Desktop\\TV";
filedata[0] = new Object();
filedata[0].subdir = "C:\\Users\\Simon\\Desktop\\TV";
filedata[0].filename = "Episode1.mkv";

directories = [];
directories[0] = new Object();
directories[0].parent = "C:\\Users\\Simon\\Desktop\\TV";
directories[0].name = "C:\\Users\\Simon\\Desktop\\TV\\MyFavouriteShow";
//*/

var playlist = [];

var video = document.getElementById("video");

function playVideo(file) {
    return function () {
        var vsource = document.getElementById("vsource");
        var caption = document.getElementById("caption");

        var filename = file.dataset.filename;
        vsource.setAttribute("src", filename);

        var actualSrc = vsource.getAttribute("src");

        var displayFilename = actualSrc.substr(actualSrc.lastIndexOf("\\") + 1);
        caption.innerHTML = displayFilename;

        video.load();
        video.play();
    }
}

function onDirectoryClick(dir) {
    return function () {
        renderBrowser(dir.dataset.directory);
    }
}

function addBrowserItem(filelist, iconName, text, className, iconClassName, dataType, dataValue) {
    var textNode = document.createTextNode(" " + text);

    var i = document.createElement("i");
    i.setAttribute("class", iconClassName);

    var p = document.createElement("p");
    p.setAttribute("class", className);
    p.setAttribute(dataType, dataValue);
    p.appendChild(i);
    p.appendChild(textNode);

    filelist.appendChild(p);
}

function forEachElementWithClassName(className, action) {
    var list = document.getElementsByClassName(className);
    for (var i = 0; i < list.length; i++) {
        action(list[i]);
    }
}

function renderBrowser(currentDir) {
    var filelist = document.getElementById("filelist");
    filelist.innerHTML = "";

    function addBrowserDirectory(name, directory, isOpen) {
        var iconClass = "fa fa-folder";
        if (isOpen === true) {
            iconClass = "fa fa-folder-open";
        }

        addBrowserItem(filelist, "folderIcon", name, "directory", iconClass, "data-directory", directory);
    }

    if (currentDir !== basedir) {
        addBrowserDirectory(".", basedir);
    }

    directories.forEach(function (dir) {
        var dirName;
        if (dir.parent === currentDir) {
            dirName = dir.name.substr(dir.parent.length + 1);
            addBrowserDirectory(dirName, dir.name);
        }

        if (currentDir.substr(0, dir.name.length) === dir.name) {
            dirName = dir.name.substr(dir.parent.length + 1);
            addBrowserDirectory(dirName, dir.name, true);
        }
    });

    filedata.forEach(function (fd) {
        if (fd.subdir === currentDir) {
            var fileName = fd.subdir + "\\" + fd.filename;
            addBrowserItem(filelist, "fa fa-film", fd.filename, "file", "fa fa-film", "data-filename", fileName);
        }
    });


    forEachElementWithClassName("file",
        function (element) {
            element.addEventListener("click", playVideo(element));
        });

    forEachElementWithClassName("directory",
        function (element) {
            console.log("folder");
            element.addEventListener("click", onDirectoryClick(element));
        });
}

renderBrowser(basedir);