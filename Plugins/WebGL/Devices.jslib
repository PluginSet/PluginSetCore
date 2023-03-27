mergeInto(LibraryManager.library, {
    syncfs: function() {
       FS.syncfs(false, function (err) {
           if (err) {
               console.log("Error: syncfs failed!"); 
           }
       }); 
    },
    
    requestAudio: function() {
        return navigator.mediaDevices.getUserMedia({ audio: true });
    },
});