mergeInto(LibraryManager.library, {
    GetQueryParam: function (keyPtr) {
        var key = UTF8ToString(keyPtr);
        var params = new URLSearchParams(window.location.search);
        var value = params.get(key) || "";
        
        var buffer = _malloc(lengthBytesUTF8(value) + 1);
        stringToUTF8(value, buffer, lengthBytesUTF8(value) + 1);
        return buffer;
    }
});
