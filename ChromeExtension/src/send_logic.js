/*
 * Copyright 2010 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

var STATUS_LOGIN_REQUIRED = 'login_required';
var STATUS_OK = 'ok';

var req = new XMLHttpRequest();

function sendToPhone(title, url, msgType, selection, listener) {

    if (localStorage["passcode"] == null)
    {
        chrome.tabs.create({url: 'options.html'});
        return;
    }
    
  req.open('POST', "http://www.daveamenta.com/wp7api/com.davux.ChromeToWindowsPhone/", true);
  req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
  req.setRequestHeader('X-Same-Domain', 'true');  // XSRF protector

  req.onreadystatechange = function() {
    if (this.readyState == 4) {
      if (req.status == 200) {
        listener(req.responseText);
      } else {
        listener('HTTP Error Code:' + req.status);
      }
    }
  };

  var data = 'title=' + encodeURIComponent(title) + '&url=' + encodeURIComponent(url) +
      '&sel=' + encodeURIComponent(selection) + '&type=' + encodeURIComponent(msgType) +
      '&passcode=' + encodeURIComponent(localStorage["passcode"]);
  req.send(data);
}
