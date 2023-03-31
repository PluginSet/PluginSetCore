mergeInto(LibraryManager.library, {
    syncfs: function() {
       FS.syncfs(false, function (err) {
           if (err) {
               console.log("Error: syncfs failed!"); 
           } else {
               console.log("Success: syncfs completed!"); 
           }
       }); 
    },
    
    requestAudio: function() {
        return navigator.mediaDevices.getUserMedia({ audio: true });
    },
    
    requestFullScreen(): function() {
        document.getElementById('unity-container').requestFullScreen();
    },
    
});