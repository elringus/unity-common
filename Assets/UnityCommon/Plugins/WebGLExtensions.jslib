mergeInto(LibraryManager.library, {

  SyncFs: function () {
    FS.syncfs(false, function (err) { });
  },

  OpenURL: function (url, target) {
    window.open(UTF8ToString(url), UTF8ToString(target));
  }

});
