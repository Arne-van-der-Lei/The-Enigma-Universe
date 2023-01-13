var express = require('express')
var { JsonDB, Config } = require('node-json-db');
var app = express()
var db = new JsonDB(new Config("Main", true, false, '/'));

var fs = require("fs"),
  PNG = require("pngjs").PNG;

var Session = {};

app.get('/login/:username/:id', async (req, res) => {

    let username = req.params.username;
    let code = req.params.id;

    check = await db.exists(`/users/${username}`);
    if(!check){
        res.sendFile(__dirname + '/Textures/error.png');
        return;
    }

    data = await db.getData(`/users/${username}`);

    console.log(data);

    if(data.code != code){
        res.sendFile(__dirname + '/Textures/error.png');
        return;
    }

    data.CurrentIp = req.ip;

    Session[req.ip] = data;
    db.push(`/users/${username}`, data);

    res.sendFile(__dirname + `/Textures/Users/${username}-${data.code}.png`);
})

app.get('/',(req,res) => {
    console.log('User Testing connection');
    res.sendFile(__dirname + '/Textures/error.png');
})

app.get('/Map', (req,res) => {
    res.sendFile(__dirname + '/Textures/map.png');
})


app.get('/Map/edit/:x/:y',(req,res) => {
    res.sendFile(__dirname + '/Textures/map.png');

    if(!Session.hasOwnProperty(req.ip)){
        return;
    }

    Session[req.ip].pos = {'x':req.params.x,'y':req.params.x}
})

app.get('/Map/edit/height/:height', (req, res) => {
    fs.createReadStream(__dirname + '/Textures/map.png')
        .pipe(
            new PNG({
            filterType: 4,
            })
        )
        .on("parsed", function () {
            

            this.pack().pipe(fs.createWriteStream(__dirname + '/Textures/map.png'));
        });
})

app.listen(3000)