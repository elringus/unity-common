mergeInto(LibraryManager.library, {

  SyncFs: function () {
    FS.syncfs(false, function (err) { });
  },

  OpenBlank: function () {
    window.open('about:blank', '_self');
  }

});
