//*
var filedata = [];
var basedir = "C:\\Users\\Simon\\Desktop\\TV";
filedata[0] = new Object();
filedata[0].subdir = "C:\\Users\\Simon\\Desktop\\TV";
filedata[0].filename = "Episode1.mkv";
filedata[0].imagename = "Episode1.mkv.jpg";
filedata[0].title = "Episode1";
filedata[0].duration = "00:00:01";
filedata[0].uploadDate = "24 Mar 2026";

directories = [];
directories[0] = new Object();
directories[0].parent = "C:\\Users\\Simon\\Desktop\\TV";
directories[0].name = "C:\\Users\\Simon\\Desktop\\TV\\MyFavouriteShow";
//*/

var playlist = [];

var video = document.getElementById("video");
var searchText = document.getElementById("searchText");
searchText.addEventListener("input", onSearch);
var filecount = document.getElementById("fileCount");

function playVideo(file) {
    return function () {
        var vsource = document.getElementById("vsource");
        var caption = document.getElementById("caption");

        var filename = file.dataset.filename;
        var title = file.dataset.title;
        vsource.setAttribute("src", filename);

        var actualSrc = vsource.getAttribute("src");

        var displayFilename = actualSrc.substr(actualSrc.lastIndexOf("\\") + 1);
        caption.innerHTML = title;

        video.load();
        video.play();
    }
}

function onDirectoryClick(dir) {
    return function () {
        renderBrowser(dir.dataset.directory);
    }
}

function addDirectoryItem(directorylist, text, iconClassName, dataValue, elementType) {

    var textNode = document.createTextNode(" " + text);

    var i = document.createElement("i");
    i.setAttribute("class", iconClassName);

    var span = document.createElement(elementType);
    span.setAttribute("class", "directory");
    span.setAttribute("data-directory", dataValue);
    span.appendChild(i);
    span.appendChild(textNode);

    directorylist.appendChild(span);
}

function addFileItem(filelist, filename, title, duration, imagename, uploadDate) {

    var div = document.createElement("div");
    div.setAttribute("class", "video_listing");
    div.setAttribute("data-filename", filename);
    div.setAttribute("data-title", title);

    if (imagename != "") {
        var img = document.createElement("img");
        img.setAttribute("src", imagename);
        img.setAttribute("class", "video_thumbnail");

        var br = document.createElement("br");

        div.appendChild(img);
        div.appendChild(br);
    }
    
    var textNode = document.createTextNode(title);

    var p = document.createElement("p");
    p.setAttribute("class", "file");
    p.appendChild(textNode);
    
    div.appendChild(p);

    var info = "";
    if (duration != "") {
        info = "[" + duration + "]";
    }
    if (uploadDate != "") {
        if (info != "") {
            info = info + " ";
        }
        info = info + uploadDate;
    }
    
    if (info != "") {
        var infoP = document.createElement("p");
        var infoTextNode = document.createTextNode(info);
        infoP.appendChild(infoTextNode);
        div.appendChild(infoP);
    }
    
    filelist.appendChild(div);
}

function forEachElementWithClassName(className, action) {
    var list = document.getElementsByClassName(className);
    for (var i = 0; i < list.length; i++) {
        action(list[i]);
    }
}


function onSearch() {
    renderFiles((fd) => inSearchResults(fd, searchText.value));
}

function inSearchResults(fileData, searchValue) {
    return fileData.title.toLowerCase().includes(searchValue.toLowerCase());
}

function inDirectory(fileData, currentDir) {
    return fileData.subdir === currentDir;
}

function renderFiles(includeFn) {
    var filelist = document.getElementById("filelist");
    filelist.innerHTML = "";

    var count = 0;
    filedata.forEach(function (fd) {
        if (includeFn(fd)) {
            var fileName = fd.subdir + "\\" + fd.filename;
            var imageName = fd.subdir + "\\" + fd.imagename;
            addFileItem(filelist, fileName, fd.title, fd.duration, imageName, fd.uploadDate);
            count += 1;
        }
    });


    if (count === 0) {
        fileCount.innerHTML = "";
    } else if (count === 1) {
        fileCount.innerHTML = "1 file";
    } else {
        fileCount.innerHTML = count + " files";
    }

    forEachElementWithClassName("video_listing",
        function (element) {
            element.addEventListener("click", playVideo(element));
        });
}

function renderDirectories(currentDir) {
    var directorylist = document.getElementById("directorylist");
    directorylist.innerHTML = "";

    function addBrowserDirectory(name, directory, isOpen) {
        var iconClass = "fa fa-folder";
        var elementType = "p";
        if (isOpen === true) {
            iconClass = "fa fa-folder-open";
            elementType = "span";
        }

        addDirectoryItem(directorylist, name, iconClass, directory, elementType);
    }

    addBrowserDirectory("/", basedir, true);

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

    forEachElementWithClassName("directory",
        function (element) {
            console.log("folder");
            element.addEventListener("click", onDirectoryClick(element));
        });
}

function renderBrowser(currentDir) {
    renderDirectories(currentDir);
    if (currentDir === basedir) {
        renderFiles((_) => true);
    } else {
        renderFiles((fd) => inDirectory(fd, currentDir));
    }
}

renderBrowser(basedir);