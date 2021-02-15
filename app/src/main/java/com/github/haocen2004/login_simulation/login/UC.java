package com.github.haocen2004.login_simulation.login;

import android.annotation.SuppressLint;
import android.content.res.Configuration;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import com.github.haocen2004.login_simulation.Data.RoleData;
import com.github.haocen2004.login_simulation.R;
import com.tencent.bugly.crashreport.BuglyLog;

import org.json.JSONException;
import org.json.JSONObject;

import cn.gundam.sdk.shell.even.SDKEventKey;
import cn.gundam.sdk.shell.even.SDKEventReceiver;
import cn.gundam.sdk.shell.even.Subscribe;
import cn.gundam.sdk.shell.exception.AliLackActivityException;
import cn.gundam.sdk.shell.exception.AliNotInitException;
import cn.gundam.sdk.shell.open.ParamInfo;
import cn.gundam.sdk.shell.open.UCOrientation;
import cn.gundam.sdk.shell.param.SDKParamKey;
import cn.gundam.sdk.shell.param.SDKParams;
import cn.uc.gamesdk.UCGameSdk;

import static com.github.haocen2004.login_simulation.util.Tools.verifyAccount;

public class UC implements LoginImpl {

    private final AppCompatActivity activity;
    private UCGameSdk sdk;
    private String sid;
    private boolean isLogin;
    private RoleData roleData;
    private static final String TAG = "UC Login";
    private final SDKEventReceiver eventReceiver = new SDKEventReceiver() {

        @Subscribe(event = SDKEventKey.ON_INIT_SUCC)
        private void onInitSucc() throws AliNotInitException, AliLackActivityException {
            sdk.login(activity, null);
        }

        @Subscribe(event = SDKEventKey.ON_LOGIN_SUCC)
        private void onLoginSucc(String sid) {
//            System.out.println("开始登陆" + sid);
            BuglyLog.i("UCSDK", "onLoginSucc: sid:" + sid);
            setSid(sid);
            doBHLogin();
        }


    };

    public void setSid(String sid) {
        this.sid = sid;
    }

    public UC(AppCompatActivity activity) {
        this.activity = activity;
        isLogin = false;


    }

    @Override
    public void login() {
        sdk = UCGameSdk.defaultSdk();
        sdk.registerSDKEventReceiver(this.eventReceiver);
        ParamInfo gpi = new ParamInfo();

        gpi.setGameId(654463);
        if (activity.getResources().getConfiguration().orientation == Configuration.ORIENTATION_PORTRAIT) {
            gpi.setOrientation(UCOrientation.PORTRAIT);
        } else {
            gpi.setOrientation(UCOrientation.LANDSCAPE);
        }
        SDKParams sdkParams = new SDKParams();
        sdkParams.put(SDKParamKey.GAME_PARAMS, gpi);
        try {
            sdk.initSdk(activity, sdkParams);

        } catch (AliLackActivityException e) {
            e.printStackTrace();
        }
    }

    @SuppressLint("HandlerLeak")
    Handler login_handler = new Handler() {
        @Override
        public void handleMessage(@NonNull Message msg) {
            super.handleMessage(msg);
            Bundle data = msg.getData();
            String feedback = data.getString("value");
//            Logger.debug(feedback);
            BuglyLog.d(TAG, "handleMessage: " + feedback);
            JSONObject feedback_json = null;
            try {
                feedback_json = new JSONObject(feedback);
            } catch (JSONException e) {
                e.printStackTrace();
            }
//            Logger.info(feedback);
            BuglyLog.i(TAG, "handleMessage: " + feedback);
            try {
                if (feedback_json.getInt("retcode") == 0) {

                    JSONObject data_json2 = feedback_json.getJSONObject("data");
                    String combo_id = data_json2.getString("combo_id");
                    String open_id = data_json2.getString("open_id");
                    String combo_token = data_json2.getString("combo_token");
                    String account_type = data_json2.getString("account_type");
                    String data2 = data_json2.getString("data");
                    int special_tag = 1;
                    if (data2.contains("true")) {
                        special_tag = 3;
                    }

                    roleData = new RoleData(activity, open_id, "", combo_id, combo_token, "20", account_type, "uc", special_tag);

                    isLogin = true;
                    makeToast(activity.getString(R.string.login_succeed));

                } else {

                    makeToast(feedback_json.getString("message"));
                    isLogin = false;

                }
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    };

    Runnable login_runnable = new Runnable() {
        @Override
        public void run() {
            String data_json = "{\"sid\":\"" + sid + "\"}";
            Message msg = new Message();
            Bundle data = new Bundle();
            data.putString("value", verifyAccount(activity, "20", data_json));
            msg.setData(data);
            login_handler.sendMessage(msg);
        }

    };

    public void doBHLogin() {
        new Thread(login_runnable).start();
    }

    private void makeToast(String result) {
        try {
            Toast.makeText(activity, result, Toast.LENGTH_LONG).show();
        } catch (Exception e) {
            Looper.prepare();
            Toast.makeText(activity, result, Toast.LENGTH_LONG).show();
            Looper.loop();
        }
    }

    @Override
    public void logout() {
        try {
            sdk.logout(activity, null);
            isLogin = false;
        } catch (AliLackActivityException | AliNotInitException e) {
            e.printStackTrace();
            isLogin = false;
        }
    }

    @Override
    public RoleData getRole() {
        return roleData;
    }

    @Override
    public boolean isLogin() {
        return isLogin;
    }
}
