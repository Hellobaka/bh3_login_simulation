package com.github.haocen2004.login_simulation.Fragment;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import com.github.haocen2004.login_simulation.R;
import com.github.haocen2004.login_simulation.databinding.FragmentReportBinding;
import com.tencent.bugly.crashreport.CrashReport;

import static com.github.haocen2004.login_simulation.util.Tools.openUrl;

public class ReportFragment extends Fragment implements View.OnClickListener {
    private FragmentReportBinding binding;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        binding = FragmentReportBinding.inflate(inflater, container, false);
        return binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
//        openUrl("https://github.com/Haocen2004/bh3_login_simulation/issues", requireActivity());
        binding.reportGithub.setOnClickListener(this);
        binding.reportBili.setOnClickListener(this);
        binding.reportQq.setOnClickListener(this);
        binding.reportHand.setOnClickListener(this);
        super.onViewCreated(view, savedInstanceState);
    }

    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            case R.id.report_github:
                openUrl("https://afdian.net/@Haocen20004", requireActivity());
                break;
            case R.id.report_bili:
                openUrl("https://space.bilibili.com/269140934", requireActivity());
                break;
            case R.id.report_qq:
                openUrl("https://jq.qq.com/?_wv=1027&k=v4Z91CMR", requireActivity());
                break;
            case R.id.report_hand:
                CrashReport.testJavaCrash();
                break;
            default:
                Toast.makeText(requireActivity(), "Wrong Button", Toast.LENGTH_LONG).show();
        }
    }
}